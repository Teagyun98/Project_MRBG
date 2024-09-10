using Zenject;

public class GameInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        // 씬에 존재하는 GameManager를 찾아 의존성 등록
        Container.Bind<GameManager>().FromComponentInHierarchy().AsSingle();
        Container.Bind<CameraMove>().FromComponentInHierarchy().AsSingle();
    }
}
