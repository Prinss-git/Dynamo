namespace ASI.Basecode.WebApp.Models
{
    /// <summary>
    /// Model of the sort dropdown partial (_SortSelect): the selected value and the choices.
    /// The first choice is the page's default order and should have an empty value.
    /// </summary>
    public class SortMenu
    {
        public SortMenu(string selected, params (string Value, string Text)[] options)
        {
            Selected = selected ?? string.Empty;
            Options = options;
        }

        public string Selected { get; }
        public (string Value, string Text)[] Options { get; }
    }
}
