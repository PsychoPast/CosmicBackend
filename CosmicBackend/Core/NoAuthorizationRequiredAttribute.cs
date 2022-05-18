using System;
namespace CosmicBackend.Core
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    internal class NoAuthorizationRequiredAttribute : Attribute
    {
    }
}