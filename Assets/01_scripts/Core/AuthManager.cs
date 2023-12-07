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
        // 디버그용
        //DateTime lastLoginDateTime = new DateTime(2023, 12, 5, 12, 3, 58); // 12월 5일 오후 10시 20분
        //PlayerPrefs.SetString("00@gmail.com_LastLoginDate", lastLoginDateTime.ToString());
    }

    #region 수행평가
    // 회원가입
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
                //print(registerEmail.text + "로 회원가입\n");
                message = "Register Success!";
            }
            else
            {
                //print($"회원가입 실패! : {task.Result}");
                message = "Register Failed. . .";
            }
        }
        SubUIManager.Instance.MessageBox(message);

        registerEmail.text = "";
        registerPassword.text = "";
        checkRegisterPassword.text = "";

        yield return null;
    }

    // 로그인
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
                currentUserEmail = email; // 현재 로그인된 사람
                message = "Login Success!";

                CheckAndUpdateLoginDate(email); // 연속인지 확인

                SubUIManager.Instance.IsLogin(email);
                StartCoroutine(LoadEmoji());
                //print(loginEmail.text + " 로그인");
            }
            else
            {
                message = "Login Failed. . .";
                //print("로그인실패!");
            }
        }
        SubUIManager.Instance.MessageBox(message);
        loginEmail.text = "";
        loginPassword.text = "";

        yield return null;
    }

    // 일일보상
    private void CheckAndUpdateLoginDate(string email)
    {
        DateTime currentDate = DateTime.Now.Date; // 현재 날짜만 가져오기
        DateTime lastLoginDate = GetLastLoginDate(email).Date; // 이전 로그인 날짜의 시간 부분을 제외하고 날짜만 가져오기
        print($"current: {currentDate} , last: {lastLoginDate}");

        double dayDiffer = (currentDate - lastLoginDate).TotalDays;
        //print(dayDiffer);

        if (dayDiffer == 1) // 차이가 1, 즉 연속적으로 로그인했을 경우
        {
            int currentContinue = PlayerPrefs.GetInt(email); // 연속성 조사
            PlayerPrefs.SetInt(email, currentContinue + 1); // 연속 로그인 횟수 +1 해주기
        }
        else // 연속 로그인 아닐 경우
        {
            PlayerPrefs.SetInt(email, 1); // 연속 초기화
        }

        PlayerPrefs.SetString($"{email}_LastLoginDate", currentDate.ToString()); // 갱신
        loginText.text = $"Attending For {PlayerPrefs.GetInt(email)} Days!";
    }
    private DateTime GetLastLoginDate(string email)
    {
        string lastLoginDateStr = PlayerPrefs.GetString($"{email}_LastLoginDate", DateTime.Now.ToString());
        DateTime lastLoginDate = DateTime.Parse(lastLoginDateStr);
        return lastLoginDate;
    }

    // 비번 바꾸기
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
                        //print($"비밀번호 변경 실패: {task2.Exception}");
                    }
                }
                else
                {
                    message = "Changing Failed. . .";
                    //print($"재인증 실패: {task.Exception}");
                }
            }
            else
            {
                message = "Changing Failed. . .";
                //print("사용자 정보 찾을 수 없음");
            }
        }
        SubUIManager.Instance.MessageBox(message);

        currentPasswordInput.text = "";
        newPasswordInput.text = "";
        confirmPasswordInput.text = "";
            
        yield return null;
    }

    // 전역
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

    // 로그아웃
    public void LogOut()
    {
        SubUIManager.Instance.OutEmoji();   
        auth.SignOut();
        currentUserEmail = ""; // 로그아웃 시 사용자 이메일 초기화
        loginText.text = "";
        SubUIManager.Instance.MessageBox("Logout, Bye!");
        SubUIManager.Instance.IsLogout();
        //print("로그아웃");
    }
    #endregion
}
