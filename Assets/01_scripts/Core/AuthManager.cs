using System.Collections;
using UnityEngine;
using System;
using TMPro;

using Firebase;
using Firebase.Auth;
using Firebase.Database;

public class AuthManager : MonoBehaviour
{
    public static AuthManager Instance;

    [SerializeField] private TextMeshProUGUI loginText;

    [Header("Login")]
    [SerializeField] private TMP_InputField loginEmail;
    [SerializeField] private TMP_InputField loginPassword;

    [Header("Register")]
    [SerializeField] private TMP_InputField registerEmail;
    [SerializeField] private TMP_InputField registerPassword;
    [SerializeField] private TMP_InputField checkRegisterPassword;


    [Header("Password Change")]
    [SerializeField] private TMP_InputField currentPasswordInput;
    [SerializeField] private TMP_InputField newPasswordInput;
    [SerializeField] private TMP_InputField confirmPasswordInput;

    [Header("Emoji")]
    [SerializeField] private Sprite[] emojiSprites;

    private FirebaseAuth auth;
    public DatabaseReference databaseRef;
    private string currentUserEmail = "";

    void Awake()
    {
        if (Instance != null) { print("AuthManager Error"); }
        Instance = this;
        auth = FirebaseAuth.DefaultInstance;
        databaseRef = FirebaseDatabase.DefaultInstance.RootReference;
    }

    private void Start()
    {
        // ����׿�
        //DateTime lastLoginDateTime = new DateTime(2023, 12, 5, 12, 3, 58); // 12�� 5�� ���� 10�� 20��
        //PlayerPrefs.SetString("00@gmail.com_LastLoginDate", lastLoginDateTime.ToString());
    }

    #region ������
    // ȸ������
    public void register()
    {
        StartCoroutine(Register(registerEmail.text, registerPassword.text, checkRegisterPassword.text));
    }
    private IEnumerator Register(string email, string password, string checkPassword)
    {
        string message = "";
        if (email.Length == 0 || password.Length == 0 || password != checkPassword)
        {
            message = "Register Failed. . .";
        }
        else
        {
            var task = auth.CreateUserWithEmailAndPasswordAsync(email, password);
            yield return new WaitUntil(() => task.IsCompleted);

            if (task.Exception != null)
            {
                message = "Register Failed. . .";
                FirebaseException fireEx = task.Exception.GetBaseException() as FirebaseException;
                AuthError errorCode = (AuthError)fireEx.ErrorCode;

                switch (errorCode)
                {
                    case AuthError.MissingEmail:
                    case AuthError.MissingPassword:
                    case AuthError.WeakPassword:
                    case AuthError.EmailAlreadyInUse:
                        break;
                }

                print(message);
            }
            else if (task.IsCompleted && !task.IsFaulted && !task.IsCanceled)
            {
                //print(registerEmail.text + "�� ȸ������\n");
                message = "Register Success!";
            }
            else
            {
                //print($"ȸ������ ����! : {task.Result}");
                message = "Register Failed. . .";
            }
        }
        SubUIManager.Instance.MessageBox(message);

        registerEmail.text = "";
        registerPassword.text = "";
        checkRegisterPassword.text = "";

        yield return null;
    }

    // �α���
    public void login()
    {
        StartCoroutine(Login(loginEmail.text, loginPassword.text));
    }
    private IEnumerator Login(string email, string password)
    {
        string message = "";

        if (email.Length==0 || password.Length == 0)
        {
            message = "Login Failed. . .";
        }
        else
        {
            var task = auth.SignInWithEmailAndPasswordAsync(email, password);
            yield return new WaitUntil(() => task.IsCompleted);
            
            if (task.IsCompleted && !task.IsFaulted && !task.IsCanceled)
            {
                currentUserEmail = email; // ���� �α��ε� ���
                message = "Login Success!";

                CheckAndUpdateLoginDate(email); // �������� Ȯ��

                SubUIManager.Instance.IsLogin(email);
                StartCoroutine(LoadEmoji());
                //print(loginEmail.text + " �α���");
            }
            else
            {
                message = "Login Failed. . .";
                //print("�α��ν���!");
            }
        }
        SubUIManager.Instance.MessageBox(message);
        loginEmail.text = "";
        loginPassword.text = "";

        yield return null;
    }

    // ���Ϻ���
    private void CheckAndUpdateLoginDate(string email)
    {
        DateTime currentDate = DateTime.Now.Date; // ���� ��¥�� ��������
        DateTime lastLoginDate = GetLastLoginDate(email).Date; // ���� �α��� ��¥�� �ð� �κ��� �����ϰ� ��¥�� ��������
        print($"current: {currentDate} , last: {lastLoginDate}");

        double dayDiffer = (currentDate - lastLoginDate).TotalDays;
        //print(dayDiffer);

        if (dayDiffer == 1) // ���̰� 1, �� ���������� �α������� ���
        {
            int currentContinue = PlayerPrefs.GetInt(email); // ���Ӽ� ����
            PlayerPrefs.SetInt(email, currentContinue + 1); // ���� �α��� Ƚ�� +1 ���ֱ�
        }
        else // ���� �α��� �ƴ� ���
        {
            PlayerPrefs.SetInt(email, 1); // ���� �ʱ�ȭ
        }

        PlayerPrefs.SetString($"{email}_LastLoginDate", currentDate.ToString()); // ����
        loginText.text = $"Attending For {PlayerPrefs.GetInt(email)} Days!";
    }
    private DateTime GetLastLoginDate(string email)
    {
        string lastLoginDateStr = PlayerPrefs.GetString($"{email}_LastLoginDate", DateTime.Now.ToString());
        DateTime lastLoginDate = DateTime.Parse(lastLoginDateStr);
        return lastLoginDate;
    }

    // ��� �ٲٱ�
    public void changePassword()
    {
        StartCoroutine(ChangePassword(currentPasswordInput.text, newPasswordInput.text, confirmPasswordInput.text));
    }
    private IEnumerator ChangePassword(string currentPassword, string newPassword, string confirmPassword)
    {
        string message = "";

        if (currentPassword.Length == 0 || newPassword.Length == 0 || confirmPassword.Length == 0 || newPassword != confirmPassword)
        {
            message = "Changing Failed. . .";
        }
        else
        {
            var user = auth.CurrentUser;

            if (user != null)
            {
                var credential = EmailAuthProvider.GetCredential(currentUserEmail, currentPassword);

                var task = user.ReauthenticateAndRetrieveDataAsync(credential);
                yield return new WaitUntil(() => task.IsCompleted);

                if (task.IsCompleted && !task.IsFaulted && !task.IsCanceled)
                {
                    var task2 = user.UpdatePasswordAsync(newPassword);
                    yield return new WaitUntil(() => task2.IsCompleted);

                    if (task2.IsCompleted && !task2.IsFaulted && !task2.IsCanceled)
                    {
                        message = "Changing Success!";
                    }
                    else
                    {
                        message = "Changing Failed. . .";
                        //print($"��й�ȣ ���� ����: {task2.Exception}");
                    }
                }
                else
                {
                    message = "Changing Failed. . .";
                    //print($"������ ����: {task.Exception}");
                }
            }
            else
            {
                message = "Changing Failed. . .";
                //print("����� ���� ã�� �� ����");
            }
        }
        SubUIManager.Instance.MessageBox(message);

        currentPasswordInput.text = "";
        newPasswordInput.text = "";
        confirmPasswordInput.text = "";
            
        yield return null;
    }

    // ����
    private IEnumerator LoadEmoji()
    {
        var dbTask = databaseRef.Child("Emoji").GetValueAsync();
        yield return new WaitUntil(() => dbTask.IsCompleted);

        if (dbTask.Exception != null)
        {
            Debug.LogWarning($"Load Task failed with {dbTask.Exception}");
        }
        else
        {
            DataSnapshot snapshot = dbTask.Result;
            if (snapshot != null && snapshot.Value != null)
            {
                Debug.Log("Load Completed");
                SetEmoji(snapshot.Value.ToString());
            }
        }
    }
    public void SetEmoji(string name)
    {
        string strEmoji = name;
        print(strEmoji);
        switch(strEmoji)
        {
            case "Happy":
            case "Music":
            case "Heart":
                SubUIManager.Instance.SetEmoji(strEmoji); // Happy Music Heart
                break;
            default:
                print("no");
                break;
        }
    }

    // �α׾ƿ�
    public void LogOut()
    {
        SubUIManager.Instance.OutEmoji();   
        auth.SignOut();
        currentUserEmail = ""; // �α׾ƿ� �� ����� �̸��� �ʱ�ȭ
        loginText.text = "";
        SubUIManager.Instance.MessageBox("Logout, Bye!");
        SubUIManager.Instance.IsLogout();
        //print("�α׾ƿ�");
    }
    #endregion
}
