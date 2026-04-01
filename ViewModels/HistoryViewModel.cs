using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DoAnCSharp.Models;
using DoAnCSharp.Services;
using System;

namespace DoAnCSharp.ViewModels;

// Class hỗ trợ gom nhóm dữ liệu theo ngày
public class HistoryGroup : List<PlayHistory>
{
    public string DateName { get; set; }
    public HistoryGroup(string dateName, List<PlayHistory> items) : base(items)
    {
        DateName = dateName;
    }
}

public partial class HistoryViewModel : ObservableObject
{
    private readonly DatabaseService _dbService;

    public ObservableCollection<HistoryGroup> GroupedHistory { get; set; } = new();

    public HistoryViewModel(DatabaseService dbService)
    {
        _dbService = dbService;
    }

    public async Task LoadHistoryAsync()
    {
        var allHistory = await _dbService.GetAllPlayHistoryAsync();

        // Phân nhóm: Hôm nay, Hôm qua, hoặc Ngày cụ thể
        var grouped = allHistory.GroupBy(x =>
        {
            if (x.PlayedAt.Date == DateTime.Today) return "Hôm nay";
            if (x.PlayedAt.Date == DateTime.Today.AddDays(-1)) return "Hôm qua";
            return x.PlayedAt.ToString("dd/MM/yyyy");
        });

        GroupedHistory.Clear();
        foreach (var group in grouped)
        {
            GroupedHistory.Add(new HistoryGroup(group.Key, group.ToList()));
        }
    }
}