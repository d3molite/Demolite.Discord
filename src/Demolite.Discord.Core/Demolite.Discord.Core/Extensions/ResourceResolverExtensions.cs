using System.Globalization;
using System.Linq.Expressions;
using System.Resources;

namespace Demolite.Discord.Core.Extensions;

public static class ResourceResolverExtensions
{
	public static string GetResource(this ResourceManager resourceManager, Expression<Func<object, string>> expression, string? culture = null)
	{
		if (expression.Body is not MemberExpression memberExpression)
			throw new ArgumentException("Expression must be a member access expression");

		var resourceKey = memberExpression.Member.Name;
		
		var cultureInfo = culture != null ? new CultureInfo(culture) : null;

		return resourceManager.GetString(resourceKey, cultureInfo)!;
	}

	public static string Format(this string input, params object?[] args)
	{
		return string.Format(input, args);
	}
}