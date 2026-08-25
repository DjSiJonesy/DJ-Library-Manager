using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DJLibraryManager.UI.Search.Models;

/// <summary>
/// Represents one metadata change that DIASISS recommends applying
/// to a physical library file.
///
/// This model represents a proposed change only. It does not modify
/// the DIASISS library and does not represent a provider search result.
/// </summary>
public sealed class MetadataChangeRecommendation : INotifyPropertyChanged
{
    private string _recommendedValue = string.Empty;

    public string Field { get; init; } = string.Empty;

    public string CurrentValue { get; init; } = string.Empty;

    /// <summary>
    /// The value currently proposed by DIASISS.
    ///
    /// The init accessor deliberately remains available so existing
    /// recommendation creation code can continue to initialise this
    /// property through object initialisers.
    ///
    /// User-driven changes are made through the explicit methods on
    /// this model so that property-change notification and
    /// IsUserModified are kept together.
    /// </summary>
    public string RecommendedValue
    {
        get => _recommendedValue;

        init
        {
            _recommendedValue =
                value ?? string.Empty;
        }
    }

    public double AgreementPercentage { get; init; }

    public int SupportingProviders { get; init; }

    public int ProvidersWithValue { get; init; }

    public MetadataConsensusStrength Strength { get; init; }

    /// <summary>
    /// Indicates whether DIASISS considers this change suitable
    /// to recommend to the user.
    /// </summary>
    public bool IsRecommended { get; init; }

    private bool _isSelected;

    /// <summary>
    /// Indicates whether this recommendation is currently selected.
    ///
    /// Property-change notification is raised so that automatic
    /// recommendation selection is immediately reflected by the UI.
    /// </summary>
    public bool IsSelected
    {
        get => _isSelected;

        set
        {
            if (_isSelected == value)
                return;

            _isSelected = value;

            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Indicates that the user has explicitly changed this
    /// recommendation.
    ///
    /// Automatic bulk selection must never overwrite a
    /// user-modified recommendation.
    /// </summary>
    public bool IsUserModified { get; private set; }

    public string Reason { get; init; } = string.Empty;

    /// <summary>
    /// Records an explicit user selection.
    /// </summary>
    public void SetUserSelection(
        bool selected)
    {
        IsSelected =
            selected;

        IsUserModified =
            true;
    }

    /// <summary>
    /// Records an explicit user change to the recommended value.
    ///
    /// This is used for user-driven metadata corrections such as
    /// swapping Artist and Title.
    /// </summary>
    public void SetUserRecommendedValue(
        string value)
    {
        value ??= string.Empty;

        if (string.Equals(
                _recommendedValue,
                value,
                StringComparison.Ordinal))
        {
            IsUserModified =
                true;

            return;
        }

        _recommendedValue =
            value;

        IsUserModified =
            true;

        OnPropertyChanged(
            nameof(RecommendedValue));

        OnPropertyChanged(
            nameof(IsChange));
    }

    /// <summary>
    /// Applies an automatic recommendation selection.
    ///
    /// User-modified recommendations are deliberately protected.
    /// </summary>
    public void SetRecommendedSelection(
        bool selected)
    {
        if (IsUserModified)
            return;

        IsSelected =
            selected;
    }

    /// <summary>
    /// Restores persisted selection state.
    /// </summary>
    public void RestoreSelection(
        bool selected,
        bool userModified)
    {
        IsSelected =
            selected;

        IsUserModified =
            userModified;
    }

    /// <summary>
    /// Explicitly resets the user decision.
    /// </summary>
    public void ResetUserSelection()
    {
        IsSelected =
            false;

        IsUserModified =
            false;
    }

    /// <summary>
    /// Swaps the proposed values between this recommendation and
    /// another recommendation.
    ///
    /// Both recommendations are marked as user modified so that
    /// subsequent automatic bulk selection cannot overwrite the
    /// user's decision.
    ///
    /// Selection state is deliberately preserved.
    /// </summary>
    public void SwapRecommendedValueWith(
        MetadataChangeRecommendation other)
    {
        ArgumentNullException.ThrowIfNull(other);

        var thisValue =
            RecommendedValue;

        var otherValue =
            other.RecommendedValue;

        SetUserRecommendedValue(
            otherValue);

        other.SetUserRecommendedValue(
            thisValue);
    }

    public bool IsChange =>
        !string.Equals(
            CurrentValue?.Trim(),
            RecommendedValue?.Trim(),
            StringComparison.OrdinalIgnoreCase);

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged(
        [CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(
            this,
            new PropertyChangedEventArgs(
                propertyName));
    }
}