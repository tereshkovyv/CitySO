namespace CitySO.Helpers;

public static class NumberLetterConverter
{
    public static string GetLetter(int col)
    {
        return col == 0 ? "" : GetLetter((col - 1) / 26) + (char)((col - 1) % 26 + 'A');
    }
}