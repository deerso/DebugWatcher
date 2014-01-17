using ReactiveUI;

namespace DebugWatcher.ViewModels
{
    public interface IAppBootstrapper : IScreen
    { }
    public class AppBootstrapper : ReactiveObject, IAppBootstrapper
    {
        public IRoutingState Router { get; private set; }
        public AppBootstrapper(IMutableDependencyResolver dependencyResolver = null, IRoutingState testRouter = null)
        {
            Router = testRouter ?? new RoutingState();

            dependencyResolver = dependencyResolver ?? RxApp.MutableResolver;

            RegisterParts(dependencyResolver);
            Router.Navigate.Execute(new MainScreenViewModel(this));
        }

        void RegisterParts(IMutableDependencyResolver dependencyResolver)
        {
            dependencyResolver.RegisterConstant(this, typeof(IScreen));

            dependencyResolver.Register(() => new MainScreenView(), typeof(IViewFor<MainScreenViewModel>));
        }
    }
}