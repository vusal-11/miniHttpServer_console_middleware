

//using System.Net;


//new Webhost(27001).Run();

//class Webhost
//{
//    int port;
//    string pathBase = @"C:/Users/user/Desktop/Step dersler/Nadir html.css/Lesson 7/code/";
//    HttpListener listener;

//    public Webhost(int port)
//    {
//        this.port = port;
//    }

//    public void Run()
//    {
//        listener = new HttpListener();
//        listener.Prefixes.Add($@"http://localhost:{port}/");
//        listener.Start();
//        Console.WriteLine($"Http server started on {port}");
//        while (true)
//        {
//            var context = listener.GetContext();
//            Task.Run(() =>
//            {
//                HandleRequest(context);
//            });
//        }


//    }


//    private void HandleRequest(HttpListenerContext context)
//    {

//        var url = context.Request.RawUrl;
//        var path = $@"{pathBase}{url.Split('/').Last()}";

//        var responce = context.Response;

//        StreamWriter streamWriter = new StreamWriter(responce.OutputStream);

//       try
//       {

//            var src = File.ReadAllText(path);
//            streamWriter.Write(src);
//       }
//       catch(Exception ex)
//       {
//            //Console.WriteLine(ex.Message);
//            var src = $@"{pathBase}404.html";
//            streamWriter.Write(src);
//       }
//       finally
//       {
//            streamWriter.Close();
//       }

//    }


//}


////////////// CoR  Chain of Responsibility



Human human = new Human("mr.13","qwertyuiop","zamanov@itstep.org");
var director = new CheckerDirector();
Console.WriteLine(director.MakeHumanChecker(human));


interface IChecker
{
    public IChecker Next { get; set; }

    public bool Check(object request);

}



abstract class BaseChecker : IChecker
{
    public IChecker Next { get ; set; }

    public abstract bool Check(object request);
}

class EmailChecker : BaseChecker
{
    public override bool Check(object request)
    {

        if (request is Human human)
        {
            return !string.IsNullOrEmpty(human.Email) && human.Email.Contains("@");
        }
        return false;



    }
}

class PasswordChecker : BaseChecker
{
    public override bool Check(object request)
    {
        if(request is Human human)
        {
            if (!string.IsNullOrWhiteSpace(human.Password) && human.Password.Length>8)
            {
                return Next.Check(request);
            }
        }
        return false;
    }
}


class UserNameChecker : BaseChecker
{
    public override bool Check(object request)
    {
        if (request is Human human)
        {
            if (!string.IsNullOrWhiteSpace(human.UserName))
            {
                return Next.Check(request);
            }
        }
        return false;
    }
}


interface ICheckerBuilder
{
    public BaseChecker EmailChecker { get; set; }
    public BaseChecker UserNameChecker { get; set; }
    public BaseChecker PAsswordChecker { get; set; }
}



class CheckerDirector
{
    public ICheckerBuilder Builder { get; set; }

    public bool MakeHumanChecker(Human human)
    {

        UserNameChecker userNameChecker = new();
        PasswordChecker passwordChecker = new();
        EmailChecker emailChecker = new();
        userNameChecker.Next = passwordChecker;
        passwordChecker.Next = emailChecker;
        return userNameChecker.Check(human);

    }


}



class Human
{
    public string UserName { get; set; }
    public string Password { get; set; }
    public string Email { get; set; }

    public Human(string userName, string password, string email)
    {
        UserName = userName;
        Password = password;
        Email = email;
    }

}
