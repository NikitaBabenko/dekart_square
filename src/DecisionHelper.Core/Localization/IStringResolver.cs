namespace DecisionHelper.Core.Localization;

public interface IStringResolver
{
    string this[string key, string locale] { get; }
    string Format(string key, string locale, params object[] args);
}
