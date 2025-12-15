using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Notification;
using Microsoft.Extensions.DependencyInjection;
using Music_Synchronizer.MVVM.View;
using Music_Synchronizer.MVVM.ViewModel;
using Music_Synchronizer.Services;
using Music_Synchronizer.Services.Storage;

namespace Music_Synchronizer;

public partial class App : Application {
    public override void Initialize() {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted() {

        var services = new ServiceCollection();
        services.AddSingleton<YoutubeDownloadService>();
        services.AddSingleton<MainWindowViewModel>();
        services.AddSingleton<HostViewModel>();
        services.AddSingleton<ClientViewModel>();

        services.AddSingleton<MusicHost>();
        services.AddSingleton<MusicClient>();
        services.AddSingleton<UserSettingsStorageService>();

        var serviceProvider = services.BuildServiceProvider();
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
            desktop.MainWindow = new MainWindowView() {
                DataContext = serviceProvider.GetRequiredService<MainWindowViewModel>(),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}