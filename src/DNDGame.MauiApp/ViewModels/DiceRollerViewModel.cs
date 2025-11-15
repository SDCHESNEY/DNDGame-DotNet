using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DNDGame.Core.Interfaces;
using DNDGame.Core.Models;
using DNDGame.MauiApp.Interfaces;

namespace DNDGame.MauiApp.ViewModels;

public partial class DiceRollerViewModel : ObservableObject
{
    private readonly IDiceRoller _diceRoller;
    private readonly INotificationService _notificationService;

    [ObservableProperty]
    private string formula = "1d20";

    [ObservableProperty]
    private DiceRollResult? lastResult;

    [ObservableProperty]
    private List<DiceRollResult> rollHistory = new();

    [ObservableProperty]
    private bool isRolling;

    public DiceRollerViewModel(IDiceRoller diceRoller, INotificationService notificationService)
    {
        _diceRoller = diceRoller;
        _notificationService = notificationService;
    }

    [RelayCommand]
    private async Task RollDiceAsync()
    {
        if (IsRolling || string.IsNullOrWhiteSpace(Formula)) return;

        IsRolling = true;

        try
        {
            await Task.Delay(500); // Add dramatic pause
            
            var result = _diceRoller.Roll(Formula);
            LastResult = result;
            
            var historyList = RollHistory.ToList();
            historyList.Insert(0, result);
            
            // Keep only last 50 rolls
            if (historyList.Count > 50)
            {
                historyList.RemoveAt(historyList.Count - 1);
            }
            
            RollHistory = historyList;

            // Show notification for critical results
            if (result.Total == 20 && Formula.Contains("1d20"))
            {
                await _notificationService.ShowNotificationAsync("Critical Hit!", "Natural 20! 🎉");
            }
            else if (result.Total == 1 && Formula.Contains("1d20"))
            {
                await _notificationService.ShowNotificationAsync("Critical Fumble!", "Natural 1! 💀");
            }
        }
        catch (Exception ex)
        {
            await _notificationService.ShowNotificationAsync("Roll Error", $"Invalid dice formula: {ex.Message}");
        }
        finally
        {
            IsRolling = false;
        }
    }

    [RelayCommand]
    private async Task RollWithAdvantageAsync()
    {
        if (IsRolling) return;

        IsRolling = true;

        try
        {
            await Task.Delay(500);
            
            var result = _diceRoller.RollWithAdvantage(Formula);
            LastResult = result;
            
            var historyList = RollHistory.ToList();
            historyList.Insert(0, result);
            
            if (historyList.Count > 50)
            {
                historyList.RemoveAt(historyList.Count - 1);
            }
            
            RollHistory = historyList;

            await _notificationService.ShowNotificationAsync("Advantage Roll", $"Rolled with advantage: {result.Total}");
        }
        catch (Exception ex)
        {
            await _notificationService.ShowNotificationAsync("Roll Error", $"Invalid dice formula: {ex.Message}");
        }
        finally
        {
            IsRolling = false;
        }
    }

    [RelayCommand]
    private async Task RollWithDisadvantageAsync()
    {
        if (IsRolling) return;

        IsRolling = true;

        try
        {
            await Task.Delay(500);
            
            var result = _diceRoller.RollWithDisadvantage(Formula);
            LastResult = result;
            
            var historyList = RollHistory.ToList();
            historyList.Insert(0, result);
            
            if (historyList.Count > 50)
            {
                historyList.RemoveAt(historyList.Count - 1);
            }
            
            RollHistory = historyList;

            await _notificationService.ShowNotificationAsync("Disadvantage Roll", $"Rolled with disadvantage: {result.Total}");
        }
        catch (Exception ex)
        {
            await _notificationService.ShowNotificationAsync("Roll Error", $"Invalid dice formula: {ex.Message}");
        }
        finally
        {
            IsRolling = false;
        }
    }

    [RelayCommand]
    private void SetQuickDice(string diceType)
    {
        Formula = diceType switch
        {
            "d4" => "1d4",
            "d6" => "1d6",
            "d8" => "1d8",
            "d10" => "1d10",
            "d12" => "1d12",
            "d20" => "1d20",
            "d100" => "1d100",
            _ => Formula
        };
    }

    [RelayCommand]
    private void ClearHistory()
    {
        RollHistory = new List<DiceRollResult>();
    }
}