namespace WebApiDemo7.MinimalApiGroup
{
    public static class MinimalApiGroupExtensions
    {
        public static RouteGroupBuilder MapTodosApi(this RouteGroupBuilder group)
        {
            group.MapGet("/", () => "Hello World");
            group.MapGet("/{id}", () => "Hello World");
            group.MapPost("/", () => "Hello World");
            group.MapPut("/{id}", () => "Hello World");
            group.MapDelete("/{id}", () => "Hello World");

            return group;
        }

       

    } 

}
