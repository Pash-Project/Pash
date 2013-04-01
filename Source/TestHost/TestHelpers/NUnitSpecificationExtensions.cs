
using System;
using System.Collections;
using NUnit.Framework;

public delegate void MethodThatThrows();

public static class NUnitSpecificationExtensions
{
    public static void ShouldBeFalse(this bool condition, string message)
    {
        Assert.IsFalse(condition, message);
    }
    public static void ShouldBeFalse(this bool condition)
    {
        ShouldBeFalse(condition, string.Empty);
    }


    public static void ShouldBeTrue(this bool condition, string message)
    {
        Assert.IsTrue(condition, message);
    }

    public static void ShouldBeTrue(this bool condition)
    {
        ShouldBeTrue(condition, string.Empty);
    }

    public static T ShouldEqual<T>(this T actual, T expected)
    {
        return ShouldEqual(actual, expected, string.Empty);
    }

    public static T ShouldEqual<T>(this T actual, T expected, string message)
    {
        Assert.AreEqual(expected, actual, message);
        return actual;
    }

    public static T ShouldNotEqual<T>(this T actual, T expected)
    {
        Assert.AreNotEqual(expected, actual);
        return actual;
    }

    public static void ShouldBeNull(this object anObject)
    {
        Assert.IsNull(anObject);
    }

    public static T ShouldNotBeNull<T>(this T anObject)
    {
        Assert.IsNotNull(anObject);
        return anObject;
    }

    public static T ShouldNotBeNull<T>(this T anObject, string message)
    {
        Assert.IsNotNull(anObject, message);
        return anObject;
    }

    public static object ShouldBeTheSameAs(this object actual, object expected)
    {
        Assert.AreSame(expected, actual);
        return expected;
    }

    public static object ShouldNotBeTheSameAs(this object actual, object expected)
    {
        Assert.AreNotSame(expected, actual);
        return expected;
    }

    public static void ShouldBeOfType(this object actual, Type expected)
    {
        Assert.IsInstanceOf(expected, actual);
    }

    public static void ShouldNotBeOfType(this object actual, Type expected)
    {
        Assert.IsNotInstanceOf(expected, actual);
    }

    public static void ShouldContain(this IList actual, object expected)
    {
        Assert.Contains(expected, actual);
    }

    public static void ShouldNotContain(this IList collection, object expected)
    {
        CollectionAssert.DoesNotContain(collection, expected);
    }

    public static IComparable ShouldBeGreaterThan(this IComparable arg1, IComparable arg2, string message = "")
    {
        Assert.Greater(arg1, arg2, message);
        return arg2;
    }

    public static IComparable ShouldBeLessThan(this IComparable arg1, IComparable arg2, string message = "")
    {
        Assert.Less(arg1, arg2, message);
        return arg2;
    }

    public static IComparable ShouldBeGreaterOrEqualThan(this IComparable arg1, IComparable arg2, string message = "")
    {
        Assert.GreaterOrEqual(arg1, arg2, message);
        return arg2;
    }

    public static IComparable ShouldBeLessOrEqualThan(this IComparable arg1, IComparable arg2, string message = "")
    {
        Assert.LessOrEqual(arg1, arg2, message);
        return arg2;
    }

    public static void ShouldBeEmpty(this ICollection collection)
    {
        Assert.IsEmpty(collection);
    }

    public static void ShouldBeEmpty(this string aString)
    {
        Assert.IsEmpty(aString);
    }

    public static void ShouldNotBeEmpty(this ICollection collection)
    {
        Assert.IsNotEmpty(collection);
    }

    public static void ShouldNotBeEmpty(this string aString)
    {
        Assert.IsNotEmpty(aString);
    }

    public static string ShouldContain(this string actual, string expected)
    {
        StringAssert.Contains(expected, actual);
        return actual;
    }

    public static void ShouldNotContain(this string actual, string expected)
    {
        try
        {
            StringAssert.Contains(expected, actual);
        }
        catch (AssertionException)
        {
            return;
        }

        throw new AssertionException(String.Format("\"{0}\" should not contain \"{1}\".", actual, expected));
    }

    public static string ShouldBeEqualIgnoringCase(this string actual, string expected)
    {
        StringAssert.AreEqualIgnoringCase(expected, actual);
        return expected;
    }

    public static void ShouldStartWith(this string actual, string expected)
    {
        StringAssert.StartsWith(expected, actual);
    }

    public static void ShouldEndWith(this string actual, string expected)
    {
        StringAssert.EndsWith(expected, actual);
    }

    public static void ShouldBeSurroundedWith(this string actual, string expectedStartDelimiter, string expectedEndDelimiter)
    {
        StringAssert.StartsWith(expectedStartDelimiter, actual);
        StringAssert.EndsWith(expectedEndDelimiter, actual);
    }

    public static void ShouldBeSurroundedWith(this string actual, string expectedDelimiter)
    {
        StringAssert.StartsWith(expectedDelimiter, actual);
        StringAssert.EndsWith(expectedDelimiter, actual);
    }

    public static void ShouldContainErrorMessage(this Exception exception, string expected)
    {
        StringAssert.Contains(expected, exception.Message);
    }

    public static Exception ShouldBeThrownBy(this Type exceptionType, MethodThatThrows method)
    {
        Exception exception = method.GetException();

        if (exception == null)
            Assert.Fail("Method " + method.Method.Name + " failed to throw expected exception [" + exceptionType.Name + "].");
        Assert.AreEqual(exceptionType, exception.GetType());

        return exception;
    }

    public static Exception GetException(this MethodThatThrows method)
    {
        Exception exception = null;

        try
        {
            method();
        }
        catch (Exception e)
        {
            exception = e;
        }

        return exception;
    }


    public static System.Management.Path ShouldEqual(this System.Management.Path inputPath, System.Management.Path expectedPath, string message = null)
    {
        Assert.AreEqual((string)expectedPath, (string)inputPath, message);
        return inputPath;
    }

    [Obsolete("We need to find a way to run distinct tests per OS (or at least windows/unix). This function is a hack to get tests to run in both places")]
    public static string PathShouldEqual(this System.Management.Path actual, System.Management.Path expected, string message = null)
    {
        return PathShouldEqual((string)actual, (string)expected, message);
    }

    [Obsolete("We need to find a way to run distinct tests per OS (or at least windows/unix). This function is a hack to get tests to run in both places")]
    public static string PathShouldEqual(this string actual, string expected, string message = null)
    {
        expected = expected.Replace("C:\\", "/").Replace(System.IO.Path.AltDirectorySeparatorChar, System.IO.Path.DirectorySeparatorChar);
        actual = actual.Replace("C:\\", "/").Replace(System.IO.Path.AltDirectorySeparatorChar, System.IO.Path.DirectorySeparatorChar);
        return ShouldEqual(actual, expected, message);
    }

}