namespace Dashboard.Tests;

/// <summary>
/// The one currency the dashboard treats specially, the US dollar, is named once - <see cref="CostText.Dollars" /> -
/// and every place that bills in dollars says so by that name rather than by a bare literal. These tests pin the
/// name and its value, and that the dollar wording the work page says goes through it.
/// </summary>
public class CostTextDollarsTests
{
    /// <summary>The named constant is the code it is, word for word: a change of it is a change of the page's words.</summary>
    [Fact]
    public void TheDollarsCodeIsTheOneThePageBillsIn() =>
        Assert.Equal("USD", CostText.Dollars);

    /// <summary>A dollar amount is said with a leading $, and the code is matched by sense of case and by nothing
    /// else: lowercase still gets the symbol, exactly as the literal the constant now stands for did.</summary>
    [Fact]
    public void ADollarAmountIsSaidWithASymbolHoweverTheCodeSpellsIt()
    {
        Assert.Equal("$1.50", CostText.Money(1.5, CostText.Dollars));
        Assert.Equal(CostText.Money(1.5, CostText.Dollars), CostText.Money(1.5, CostText.Dollars.ToLowerInvariant()));
    }

    /// <summary>The work page bills in dollars by contract, so an item's own cost is said with the symbol and a cost
    /// the file never metered is the not-metered word, never a bare number with a code after it.</summary>
    [Fact]
    public void AWorkItemsOwnCostIsADollarAmountAndANotMeteredOneIsTheWord()
    {
        Assert.StartsWith("$", WorkItemsText.ItemCost(2.0), StringComparison.Ordinal);
        Assert.Equal(WorkItemsText.NotMetered, WorkItemsText.ItemCost(null));
    }
}
