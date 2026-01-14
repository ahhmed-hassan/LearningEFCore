// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");

public interface IMiddleware{
    public bool next(HttpContent httpContent);
}

public class FirstMiddleware : IMiddleware{
    public bool next(HttpContent httpContent) => /*Asszme some work*/true;
}

public class SecondMiddleware : IMiddleware { 
    public bool next(HttpContent httpContent) => /*Assume some different work*/false;
}

public class MiddlewareContainer{
    private List<IMiddleware> _middlewares = new List<IMiddleware>();
    public void AddMiddleware(IMiddleware middleware) => _middlewares.Add(middleware);
    public bool process(HttpContent httpContent){
        foreach (var middleware in _middlewares){
            if(!middleware.next(httpContent))
                return false;
        }
        return true;
    }
}