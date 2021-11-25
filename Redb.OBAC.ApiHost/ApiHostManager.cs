namespace Redb.OBAC.ApiHost
{
    public class ApiHostManager
    {

        public static ApiHostImpl CreateApiHost(IObacConfiguration configuration)
        {
            var apiHost = new ApiHostImpl(configuration);

            // todo
            // builder.RegisterType<ApiServerImpl>().AsSelf().SingleInstance();
            // builder.Register(c =>
            // {
            //     var instance = c.Resolve<ApiServerImpl>();
            //     return ObacServer.BindService(instance);
            // }).As<ServerServiceDefinition>();
            //
            // builder.RegisterType<GrpcServerExecutable>().SingleInstance();

            return apiHost;
        }
    }
}