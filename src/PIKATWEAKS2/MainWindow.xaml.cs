using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using PIKATWEAKS2.Models;
using PIKATWEAKS2.Services;
namespace PIKATWEAKS2;

public partial class MainWindow : Window
{
    readonly CommandRunner runner = new();
    readonly BackupService backup = new();
    readonly ObservableCollection<TweakRow> rows = new();
    readonly ICollectionView view;
    public MainWindow()
    {
        InitializeComponent();
        foreach(var t in TweakCatalog.All) rows.Add(new TweakRow(t));
        view = CollectionViewSource.GetDefaultCollectionView(rows);
        view.Filter = Filter;
        TweakList.ItemsSource = view;
        CategoryBox.Items.Add("All Categories");
        foreach(var c in TweakCatalog.All.Select(x=>x.Category).Distinct().OrderBy(x=>x)) CategoryBox.Items.Add(c);
        CategoryBox.SelectedIndex=0;
        TweakCount.Text=TweakCatalog.All.Count.ToString();
        RecommendedCount.Text=TweakCatalog.All.Count(x=>x.Recommended).ToString();
        CpuCount.Text=Environment.ProcessorCount.ToString();
        AddLog("PIKATWEAKS2 ready. Administrator privileges are active.");
        AddLog($"Loaded {TweakCatalog.All.Count} source-derived V10.2 actions.");
    }
    bool Filter(object o)
    {
        if(o is not TweakRow r) return false;
        var q=SearchBox?.Text?.Trim() ?? "";
        var cat=CategoryBox?.SelectedItem?.ToString() ?? "All Categories";
        return (cat=="All Categories" || r.Model.Category==cat) && (q.Length==0 || r.Model.Name.Contains(q,StringComparison.OrdinalIgnoreCase) || r.Model.Description.Contains(q,StringComparison.OrdinalIgnoreCase) || r.Model.Category.Contains(q,StringComparison.OrdinalIgnoreCase));
    }
    void RefreshView(){view.Refresh();}
    void AddLog(string s){LogList.Items.Insert(0,$"[{DateTime.Now:HH:mm:ss}] {s}"); if(LogList.Items.Count>100) LogList.Items.RemoveAt(100);}
    void SetStatus(string s){StatusCard.Text=s; PageSubtitle.Text=s;}
    async Task ApplyAsync(IEnumerable<TweakRow> selected, string mode)
    {
        var list=selected.Where(x=>x.Selected).ToList();
        if(list.Count==0){MessageBox.Show("Select at least one tweak.","PIKATWEAKS2");return;}
        if(list.Any(x=>x.Model.Maintenance) && mode=="all")
        {
            MessageBox.Show("Maintenance and diagnostic actions are excluded from this operation. Run them individually from the list.","PIKATWEAKS2");
            return;
        }
        var ask=MessageBox.Show($"PIKATWEAKS2 will apply {list.Count} action(s). A registry backup will be created first. Continue?","Apply tweaks",MessageBoxButton.YesNo,MessageBoxImage.Warning);
        if(ask!=MessageBoxResult.Yes)return;
        try
        {
            SetStatus("Creating backup...");
            var folder=await backup.CreateAsync(runner); AddLog("Backup created: "+folder);
            foreach(var row in list)
            {
                SetStatus("Applying: "+row.Model.Name); AddLog("Applying "+row.Model.Name);
                var result=await runner.RunAsync(row.Model.Command);
                if(result.ExitCode==0){AddLog("✓ "+row.Model.Name); row.Selected=false;}
                else AddLog("✗ "+row.Model.Name+" (exit "+result.ExitCode+")");
                if(!string.IsNullOrWhiteSpace(result.Output)) AddLog(result.Output.Trim().Replace("\r"," ").Replace("\n"," ").Substring(0,Math.Min(180,result.Output.Trim().Length)));
            }
            SetStatus("Complete");
            if(list.Any(x=>x.Model.RequiresRestart)) AddLog("Some changes may require a Windows restart.");
        }
        catch(Exception ex){AddLog("ERROR: "+ex.Message); SetStatus("Error"); MessageBox.Show(ex.Message,"PIKATWEAKS2",MessageBoxButton.OK,MessageBoxImage.Error);}
        RefreshView();
    }
    async void ApplySelected_Click(object s,RoutedEventArgs e)=>await ApplyAsync(rows,"selected");
    async void Optimize_Click(object s,RoutedEventArgs e){foreach(var r in rows)r.Selected=r.Model.Recommended; await ApplyAsync(rows.Where(r=>r.Model.Recommended),"recommended");}
    async void ApplyAll_Click(object s,RoutedEventArgs e){foreach(var r in rows)r.Selected=!r.Model.Maintenance && r.Model.Category!="Diagnostics"; await ApplyAsync(rows.Where(r=>r.Selected),"all");}
    void SelectAll_Click(object s,RoutedEventArgs e){foreach(var r in rows)r.Selected=true;RefreshView();}
    void Clear_Click(object s,RoutedEventArgs e){foreach(var r in rows)r.Selected=false;RefreshView();}
    void Search_TextChanged(object s,TextChangedEventArgs e)=>RefreshView();
    void Category_Changed(object s,SelectionChangedEventArgs e)=>RefreshView();
    void ShowCategory(string category){CategoryBox.SelectedItem=category; PageTitle.Text=category; PageSubtitle.Text=$"{category} tweaks";}
    void Home_Click(object s,RoutedEventArgs e){CategoryBox.SelectedIndex=0;PageTitle.Text="Dashboard";PageSubtitle.Text="Tune Windows without a command window.";}
    void Tweaks_Click(object s,RoutedEventArgs e){CategoryBox.SelectedIndex=0;PageTitle.Text="Tweaks";PageSubtitle.Text="All PIKATWEAKS2 actions";}
    void Gaming_Click(object s,RoutedEventArgs e)=>ShowCategory("Gaming");
    void Cleanup_Click(object s,RoutedEventArgs e){CategoryBox.SelectedItem="Cleanup";PageTitle.Text="Cleanup";PageSubtitle.Text="Temporary files and maintenance";}
    void Network_Click(object s,RoutedEventArgs e)=>ShowCategory("Network");
    void Privacy_Click(object s,RoutedEventArgs e)=>ShowCategory("Privacy");
    void Services_Click(object s,RoutedEventArgs e)=>ShowCategory("Services");
    async void Backup_Click(object s,RoutedEventArgs e){try{var folder=await backup.CreateAsync(runner);AddLog("Manual backup created: "+folder);MessageBox.Show("Backup created at:\n"+folder,"PIKATWEAKS2");}catch(Exception ex){MessageBox.Show(ex.Message);}}
    async void Restore_Click(object s,RoutedEventArgs e)
    {
        try
        {
            if(!Directory.Exists(backup.Folder)){MessageBox.Show("No backups found.");return;}
            var latest=new DirectoryInfo(backup.Folder).GetDirectories().OrderByDescending(x=>x.Name).FirstOrDefault();
            if(latest==null){MessageBox.Show("No backups found.");return;}
            if(MessageBox.Show("Restore the latest registry backup? This can revert Windows settings.","Restore",MessageBoxButton.YesNo,MessageBoxImage.Warning)!=MessageBoxResult.Yes)return;
            foreach(var f in latest.GetFiles("*.reg")) await runner.RunAsync($"reg import \"{f.FullName}\"");
            AddLog("Restored backup: "+latest.FullName);MessageBox.Show("Backup restored. Some settings may require sign-out or restart.");
        }catch(Exception ex){MessageBox.Show(ex.Message,"Restore error");}
    }
    void Refresh_Click(object s,RoutedEventArgs e){RefreshView();AddLog("View refreshed.");}
    public sealed class TweakRow:INotifyPropertyChanged
    {
        public Tweak Model{get;} public TweakRow(Tweak m)=>Model=m;
        bool selected; public bool Selected{get=>selected;set{if(selected==value)return;selected=value;PropertyChanged?.Invoke(this,new(nameof(Selected)));}}
        public string Name=>Model.Name; public string Description=>Model.Description; public string Category=>Model.Category; public bool Recommended=>Model.Recommended;
        public Visibility RecommendedVisibility=>Model.Recommended?Visibility.Visible:Visibility.Collapsed;
        public event PropertyChangedEventHandler? PropertyChanged; void OnPropertyChanged([CallerMemberName]string? n=null)=>PropertyChanged?.Invoke(this,new(n));
    }
}
