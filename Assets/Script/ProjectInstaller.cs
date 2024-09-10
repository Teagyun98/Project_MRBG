using UnityEngine;
using Zenject;

public class ProjectInstaller : MonoInstaller
{
    [SerializeField] private UserDataManager udm;

    public override void InstallBindings()
    {
        Container.Bind<UserDataManager>().FromComponentInNewPrefab(udm).AsSingle().NonLazy();
    }
}