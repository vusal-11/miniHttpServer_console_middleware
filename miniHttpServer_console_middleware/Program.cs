

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



//Human human = new Human("mr.13","qwertyuiop","zamanov@itstep.org");
//var director = new CheckerDirector();
//Console.WriteLine(director.MakeHumanChecker(human));


//interface IChecker
//{
//    public IChecker Next { get; set; }

//    public bool Check(object request);

//}



//abstract class BaseChecker : IChecker
//{
//    public IChecker Next { get ; set; }

//    public abstract bool Check(object request);
//}

//class EmailChecker : BaseChecker
//{
//    public override bool Check(object request)
//    {

//        if (request is Human human)
//        {
//            return !string.IsNullOrEmpty(human.Email) && human.Email.Contains("@");
//        }
//        return false;



//    }
//}

//class PasswordChecker : BaseChecker
//{
//    public override bool Check(object request)
//    {
//        if(request is Human human)
//        {
//            if (!string.IsNullOrWhiteSpace(human.Password) && human.Password.Length>8)
//            {
//                return Next.Check(request);
//            }
//        }
//        return false;
//    }
//}


//class UserNameChecker : BaseChecker
//{
//    public override bool Check(object request)
//    {
//        if (request is Human human)
//        {
//            if (!string.IsNullOrWhiteSpace(human.UserName))
//            {
//                return Next.Check(request);
//            }
//        }
//        return false;
//    }
//}


//interface ICheckerBuilder
//{
//    public BaseChecker EmailChecker { get; set; }
//    public BaseChecker UserNameChecker { get; set; }
//    public BaseChecker PAsswordChecker { get; set; }
//}



//class CheckerDirector
//{
//    public ICheckerBuilder Builder { get; set; }

//    public bool MakeHumanChecker(Human human)
//    {

//        UserNameChecker userNameChecker = new();
//        PasswordChecker passwordChecker = new();
//        EmailChecker emailChecker = new();
//        userNameChecker.Next = passwordChecker;
//        passwordChecker.Next = emailChecker;
//        return userNameChecker.Check(human);

//    }


//}



//class Human
//{
//    public string UserName { get; set; }
//    public string Password { get; set; }
//    public string Email { get; set; }

//    public Human(string userName, string password, string email)
//    {
//        UserName = userName;
//        Password = password;
//        Email = email;
//    }

//}




//////// Middleware like







using System.Net;




WebHost host = new WebHost(27001);
host.UseStartUp<StartUp>();
host.Run();




public delegate void HttpHandler(HttpListenerContext context);
public interface IMiddleware
{
    public HttpHandler Next { get; set; }
    public void Handle (HttpListenerContext context);
}



public class LoggerMiddleware : IMiddleware
{
    public HttpHandler Next { get; set;  }

    public void Handle(HttpListenerContext context)
    {
        Console.WriteLine($@"{context.Request.HttpMethod}
{context.Request.RawUrl}
{context.Request.RemoteEndPoint}");
        Next.Invoke(context);
    }

}



public class StaticFilesMiddleware : IMiddleware
{
    public HttpHandler Next { get; set; }

    public void Handle(HttpListenerContext context)
    {
        
        if(Path.HasExtension(context.Request.RawUrl))
        {

            try
            {
                var fileName = context.Request.RawUrl.Substring(1);
                var path = $@"C:\Users\user\source\repos\miniHttpServer_console_middleware\miniHttpServer_console_middleware\wwwroot\{fileName}";
                var bytes = File.ReadAllBytes(path);
                if (Path.GetExtension(path) == "html")
                {
                    context.Response.AddHeader("Content-Type", "text/html");
                }
                else if(Path.GetExtension(path) =="jpg")
                {
                    context.Response.AddHeader("Content-Type", "image/jpg");

                }
                context.Response.OutputStream.Write(bytes,0,bytes.Length);
            }
            catch (Exception)
            {

                context.Response.StatusCode = 404;
                context.Response.StatusDescription = "File not found";
               
            }
            

        }
        else
        {
            Next.Invoke(context);
        }

        context.Response.Close();


    }

}



public interface IStartup
{
    public void Configure(MiddlewareBuilder builder);
}

class StartUp : IStartup
{
    public void Configure(MiddlewareBuilder builder)
    {
        builder.Use<LoggerMiddleware>();
        builder.Use<StaticFilesMiddleware>();
    }
}

public class MiddlewareBuilder
{

    private Stack<Type> middlewares = new Stack<Type>();
    public void Use<T>() where T : IMiddleware
    {
        middlewares.Push(typeof(T));
    }

    public HttpHandler Build()
    {
        HttpHandler handler = (context) => context.Response.Close();
        while (middlewares.Count != 0)
        {

            var middlware = middlewares.Pop();
            IMiddleware? middleWare = Activator.CreateInstance(middlware) as IMiddleware;
            middleWare.Next = handler;
            handler = middleWare.Handle;

        }
        return handler;
    }
}


public class WebHost
{
    private int _port;
    private HttpHandler _handler;
    private HttpListener _listener;
    private MiddlewareBuilder _middlewareBuilder = new();


    public WebHost(int port)
    {
        _port = port;
        _listener = new HttpListener();
        _listener.Prefixes.Add($"http://localhost:{_port}/");
    }


    public void UseStartUp<T>() where T :IStartup, new()
    {
        IStartup startup = new T();
        startup.Configure(_middlewareBuilder);
        _handler = _middlewareBuilder.Build();
    }


    public void Run()
    {
        _listener.Start();
        Console.WriteLine($"Server started {_port}");
        while (true)
        {
            HttpListenerContext context = _listener.GetContext();
            Task.Run(() => HandleRequest(context));
            
        }
    }

    private void HandleRequest(HttpListenerContext context)
    {

        _handler.Invoke(context);

    }




}

