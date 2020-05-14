using System;
using System.Collections.Generic;
using System.Linq;
using HotChocolate.Types;

namespace HotChocolate
{
    internal sealed class SchemaTypes
    {
        private readonly Dictionary<NameString, INamedType> _types;
        private readonly Dictionary<NameString, List<IType>> _possibleTypes;

        public SchemaTypes(SchemaTypesDefinition definition)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            _types = definition.Types.ToDictionary(t => t.Name);
            _possibleTypes = CreatePossibleTypeLookup(definition.Types);
            QueryType = definition.QueryType;
            MutationType = definition.MutationType;
            SubscriptionType = definition.SubscriptionType;
        }

        public ObjectType QueryType { get; }
        public ObjectType MutationType { get; }
        public ObjectType SubscriptionType { get; }

        public T GetType<T>(NameString typeName) where T : IType
        {
            if (_types.TryGetValue(typeName, out INamedType namedType)
                && namedType is T type)
            {
                return type;
            }

            // TODO : resource
            throw new ArgumentException(
                $"The specified type `{typeName}` does not exist or " +
                $"is not of the specified kind `{typeof(T).Name}`.",
                nameof(typeName));
        }

        public bool TryGetType<T>(NameString typeName, out T type)
            where T : IType
        {
            if (_types.TryGetValue(typeName, out INamedType namedType)
                && namedType is T t)
            {
                type = t;
                return true;
            }

            type = default;
            return false;
        }

        public IReadOnlyCollection<INamedType> GetTypes()
        {
            return _types.Values;
        }

        public bool TryGetClrType(NameString typeName, out Type clrType)
        {
            if (_types.TryGetValue(typeName, out INamedType type)
                && type is IHasClrType ct
                && ct.ClrType != typeof(object))
            {
                clrType = ct.ClrType;
                return true;
            }

            clrType = null;
            return false;
        }

        public bool TryGetPossibleTypes(
            string abstractTypeName,
            out IReadOnlyList<IType> types)
        {
            if (_possibleTypes.TryGetValue(abstractTypeName, out List<IType> pt))
            {
                types = pt;
                return true;
            }

            types = null;
            return false;
        }

        private static Dictionary<NameString, List<IType>> CreatePossibleTypeLookup(
            IReadOnlyCollection<INamedType> types)
        {
            var possibleTypes = new Dictionary<NameString, List<IType>>();

            foreach (ObjectType objectType in types.OfType<ObjectType>())
            {
                possibleTypes[objectType.Name] = new List<IType> { objectType };

                foreach (InterfaceType interfaceType in objectType.Interfaces)
                {
                    if (!possibleTypes.TryGetValue(interfaceType.Name, out List<IType> pt))
                    {
                        pt = new List<IType>();
                        possibleTypes[interfaceType.Name] = pt;
                    }

                    pt.Add(objectType);
                }
            }

            foreach (UnionType unionType in types.OfType<UnionType>())
            {
                foreach (IType objectType in unionType.Types.Values)
                {
                    if (!possibleTypes.TryGetValue(
                        unionType.Name, out List<IType> pt))
                    {
                        pt = new List<IType>();
                        possibleTypes[unionType.Name] = pt;
                    }

                    pt.Add(objectType);
                }
            }

            foreach (InputUnionType inputUnionType in types.OfType<InputUnionType>())
            {
                foreach (IType objectType in inputUnionType.Types.Values)
                {
                    if (!possibleTypes.TryGetValue(
                        inputUnionType.Name, out List<IType> pt))
                    {
                        pt = new List<IType>();
                        possibleTypes[inputUnionType.Name] = pt;
                    }

                    pt.Add(objectType);
                }
            }

            return possibleTypes;
        }
    }
}
