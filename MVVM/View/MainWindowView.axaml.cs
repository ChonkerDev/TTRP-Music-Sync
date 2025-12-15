using Avalonia;
using Avalonia.Controls;
using Music_Synchronizer.Services;

namespace Music_Synchronizer.MVVM.View;

public partial class MainWindowView : Window {
    public MainWindowView() {
        InitializeComponent();
        Loaded += (sender, args) => {
            NotificationService.Initialize(this);

        };
    }
}