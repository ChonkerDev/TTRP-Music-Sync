using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Music_Synchronizer.MVVM.ViewModel;

namespace Music_Synchronizer.MVVM.View;

public partial class HostView : UserControl {
    public HostView() {
        InitializeComponent();
    }

    private void VolumeSlider_OnValueChanged(object? sender, RangeBaseValueChangedEventArgs e) {
        if (DataContext is HostViewModel vm) {
            vm.OnVolumeSliderChanged(e.NewValue);
        }
    }
}