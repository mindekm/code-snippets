LoggerConfiguration configuration;
configuration.DestructureWith<DestructurablePolicy>();

public class DestructurablePolicy : IDestructuringPolicy
{
    public bool TryDestructure(object value, ILogEventPropertyValueFactory propertyValueFactory, out LogEventPropertyValue result)
    {
        switch (value)
        {
            case IDestructurable destructurable:
                result = propertyValueFactory.CreatePropertyValue(destructurable.Destructure());
                return true;
            default:
                result = default;
                return false;
        }
    }
}

public interface IDestructurable<out T>
{
    T Destructure();
}

public interface IDestructurable : IDestructurable<object>
{    
}
