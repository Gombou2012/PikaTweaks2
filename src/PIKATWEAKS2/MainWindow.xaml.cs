using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using PIKATWEAKS2.Models;
using PIKATWEAKS2.Services;
namespace PIKATWEAKS2;
public partial class MainWindow : Window
{
    readonly CommandRunner runner = new();
    readonly LogService log = new();
    readonly ObservableCollection<Tweak> visible = new();
    string category = "All";
    public MainWindow(){ InitializeComponent(); Loaded += (_,__) => Refresh(); }
    void Refresh()
    {
        visible.Clear();
        var q=SearchBox?.Text?.Trim() ?? "";
        foreach(var t in TweakCatalog.All.Where(t => (category=="All" || (category=="V10.2" ? true : t.Category==category)) && (q.Length==0 || (t.Name+" "+t.Description+" "+t.Category).Contains(q,StringComparison.OrdinalIgnoreCase)))) visible.Add(t);
        TweakList.ItemsSource=visible;
        CountText.Text=$"{visible.Count} tweaks";
    }
    void Category_Click(object s,RoutedEventArgs e){ category=(string)((Button)s).Tag; Refresh(); }
    void SearchBox_TextChanged(object s,TextChangedEventArgs e){ if(IsInitialized) Refresh(); }
    async void ApplySelected_Click(object s,RoutedEventArgs e){ await ApplyAsync(TweakCatalog.All.Where(t => t.IsSelected && visible.Contains(t))); }
    async void ApplyRecommended_Click(object s,RoutedEventArgs e){ await ApplyAsync(TweakCatalog.All.Where(t=>t.Recommended)); }
    async void ApplyAll_Click(object s,RoutedEventArgs e)
    {
        var answer=MessageBox.Show("Apply all 75 source-derived V10.2 tweaks? Diagnostic-only items are included only where they change system state; destructive repair/reset actions are marked High and are excluded from Recommended.","PIKATWEAKS2",MessageBoxButton.YesNo,MessageBoxImage.Warning);
        if(answer==MessageBoxResult.Yes) await ApplyAsync(TweakCatalog.All);
    }
    async Task ApplyAsync(IEnumerable<Tweak> tweaks)
    {
        var list=tweaks.ToList(); if(!list.Any()){ MessageBox.Show("Select at least one tweak."); return; }
        foreach(var t in list)
        {
            LogList.Items.Insert(0,$"Running: {t.Name}"); log.Write($"START {t.Id} {t.Name}");
            try
            {
                var r=await runner.RunScriptAsync(t.Script);
                var ok=r.ExitCode==0;
                LogList.Items.Insert(0,ok?$"✓ {t.Name}":$"✗ {t.Name} (exit {r.ExitCode})");
                if(!string.IsNullOrWhiteSpace(r.Output)) LogList.Items.Insert(0,r.Output.Length>180?r.Output[..180]+"…":r.Output);
                log.Write($"END {t.Id} exit={r.ExitCode} {r.Output.Replace(Environment.NewLine," ")}");
            }
            catch(Exception ex){ LogList.Items.Insert(0,$"✗ {t.Name}: {ex.Message}"); log.Write($"ERROR {t.Id} {ex}"); }
        }
        MessageBox.Show("Finished. Check the Activity panel for individual results.","PIKATWEAKS2",MessageBoxButton.OK,MessageBoxImage.Information);
    }
}
