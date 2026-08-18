using Core;
using Dungeon;
using TimeControll;
using Zenject;

public class BootstrapOfDungeon : MonoInstaller
{
    public override void InstallBindings() => InitializeAndBindServices(Container);

    private void InitializeAndBindServices(DiContainer container)
    {
        container.BindInterfacesAndSelfTo<TimeController>().FromNew().AsSingle().NonLazy();
        container.BindInterfacesAndSelfTo<SimulationTicker>().FromNew().AsSingle().NonLazy();

        var dungeonConfig = DungeonGeneratorConfig.CreateInstance<DungeonGeneratorConfig>();
        container.BindInstance(dungeonConfig);

        container.BindInterfacesAndSelfTo<DungeonModel>().AsSingle();
        container.Bind<DungeonEnemySpawner>().AsSingle();
        container.BindInterfacesAndSelfTo<DungeonFacade>().AsSingle();
        container.BindInterfacesAndSelfTo<GameBootstrapper>().FromNew().AsSingle().NonLazy();
    }
}