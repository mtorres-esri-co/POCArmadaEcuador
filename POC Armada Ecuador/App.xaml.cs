using System.Windows;

using Esri.ArcGISRuntime;

using POC_Armada_Ecuador.Views;

using Prism.Ioc;

namespace POC_Armada_Ecuador {
  /// <summary>
  /// Interaction logic for App.xaml
  /// </summary>
  public partial class App {
    protected override Window CreateShell() {
      ArcGISRuntimeEnvironment.ApiKey = "AAPKb6399acfd2224420a76342b461b741a2jrhijou3bVsrlNqtOkqi_flWfX4aDYh9lzjwRn2z0jL4FsVMEr_pGDMWTcWVedGr";
      return Container.Resolve<MainWindow>();
    }

    protected override void RegisterTypes(IContainerRegistry containerRegistry) {

    }
  }
}
