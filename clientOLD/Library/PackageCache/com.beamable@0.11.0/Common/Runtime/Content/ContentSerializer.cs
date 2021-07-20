using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
// Promise library
using Beamable.Serialization.SmallerJSON;
using Beamable.Content;
// pull into common
using UnityEngine;
using UnityEngine.AddressableAssets;
using Object = System.Object;

// stub out

namespace Beamable.Common.Content
{
   public abstract class ContentSerializer<TContentBase>
   {

      class FieldInfoWrapper
      {
         public FieldInfo RawField;
         public string SerializedName;
         public string[] FormerlySerializedAs;
         public Type FieldType => RawField.FieldType;
         public void SetValue(object obj, object value) => RawField.SetValue(obj, value);
         public object GetValue(object obj) => RawField.GetValue(obj);
      }

      protected string GetNullStringForType(Type argType)
      {
         if (typeof(IList).IsAssignableFrom(argType))
         {
            return "[]";
         }

         if (!typeof(string).IsAssignableFrom(argType) && argType.GetCustomAttribute<System.SerializableAttribute>() != null)
         {
            try
            {
               var defaultInstance = Activator.CreateInstance(argType);
               return SerializeArgument(defaultInstance, argType);
            }
            catch (MissingMethodException)
            {
               return "null";
            }
         }

         return "null";
      }

      protected string SerializeArgument(object arg, Type argType)
      {
         // JSONUtility will serialize objects correctly, but doesn't handle primitives well.
         if (arg == null)
         {
            return GetNullStringForType(argType);
         }

         switch (arg)
         {
            /* MAP TYPES... */
            case IDictionary dictionary:
               var arrayDict = new ArrayDict();
               var dictionaryEnumerator = dictionary.GetEnumerator();
               while (dictionaryEnumerator.MoveNext())
               {
                  var key = dictionaryEnumerator.Key?.ToString();
                  var value = dictionaryEnumerator.Value;
                  if (!string.IsNullOrEmpty(key))
                  {
                     arrayDict.Add(key, value);
                  }
               }
               var json = Json.Serialize(arrayDict, new StringBuilder());
               return json;
            /* ARRAY TYPES... */
            case IList arr:
               var serializedArray = new PropertyValue[arr.Count];

               var index = 0;
               foreach (var elem in arr)
               {
                  var serializedElem = SerializeArgument(elem, elem?.GetType() ?? typeof(void));
                  serializedArray[index] = new PropertyValue { rawJson = serializedElem };
                  index++;
               }

               return Json.Serialize(serializedArray, new StringBuilder());

            /* PRIMITIVE TYPES... */
            case Enum e:
               return Json.Serialize(arg, new StringBuilder());
            case bool b:
            case long l:
            case string s:
            case double d:
            case float f:
            case short sh:
            case byte by:
            case int i:
               return Json.Serialize(arg, new StringBuilder());

            /* SPECIAL TYPES... */
            case IContentRef contentRef:
#if BEAMABLE_LEGACY_CONTENT_REFS
               return Json.Serialize(new ArrayDict
               {
                  {"id", contentRef.GetId()}
               }, new StringBuilder());
#else
               return Json.Serialize(contentRef.GetId(), new StringBuilder());
#endif

            case AssetReference addressable:
               var addressableDict = new ArrayDict
               {
                  {"referenceKey", addressable.AssetGUID},
               };
               if (addressable.SubObjectName != null)
               {
                  addressableDict.Add("subObjectName", addressable.SubObjectName);
               }

               return Json.Serialize(addressableDict, new StringBuilder());

            default:

               /*
                * We can't use the JsonUtility.ToJson because we can't override certain types,
                *  like optionals, addressables, links or refs.
                */
               var fields = GetFieldInfos(arg.GetType());
               var dict = new ArrayDict();
               foreach (var field in fields)
               {
                  var fieldValue = field.GetValue(arg);
                  var fieldType = field.FieldType;

                  if (fieldValue is Optional optional)
                  {
                     if (optional.HasValue)
                     {
                        fieldValue = optional.GetValue();
                        fieldType = optional.GetOptionalType();
                     }
                     else
                     {
                        continue; // skip field.
                     }
                  }
                  var fieldJson = SerializeArgument(fieldValue, fieldType);
                  dict.Add(field.SerializedName, new PropertyValue { rawJson = fieldJson });
               }

               return Json.Serialize(dict, new StringBuilder());
         }

      }


      protected object DeserializeResult(object preParsedValue, Type type)
      {

         try
         {
            if (typeof(Optional).IsAssignableFrom(type))
            {
               var optional = (Optional) Activator.CreateInstance(type);

               if (preParsedValue == null)
               {
                  optional.HasValue = false;
               }
               else
               {
                  var value = DeserializeResult(preParsedValue, optional.GetOptionalType());
                  optional.SetValue(value);
               }

               return optional;
            }

            //if (typeof(IContentLink).IsAssignableFrom())


            var json = Json.Serialize(preParsedValue, new StringBuilder());

            if (typeof(Unit).IsAssignableFrom(type))
            {
               return PromiseBase.Unit;
            }

            bool TryGetElementType(IList list, Type baseType, out Type elementType)
            {

               var hasMatchingType = baseType.GenericTypeArguments.Length == 1;
               if (hasMatchingType)
               {
                  elementType = baseType.GenericTypeArguments[0];
                  return true;
               }

               var hasBaseType = baseType.BaseType != typeof(Object);
               if (hasBaseType)
               {
                  return TryGetElementType(list, baseType.BaseType, out elementType);
               }

               if (list.Count > 0)
               {
                  var elemType = list[0].GetType();
                  elementType = elemType;
                  return true;
               }
               else
               {
                  elementType = null;
                  return true;
               }
            }

            IContentRef contentRef;
            IContentLink contentLink;
            switch (preParsedValue)
            {
               case null:
                  return null;

               /* REFERENCE TYPES */
               case ArrayDict linkDict when typeof(IContentLink).IsAssignableFrom(type):
                  contentLink = (IContentLink) Activator.CreateInstance(type);
                  object linkId = "";
                  linkDict.TryGetValue("id", out linkId);
                  contentLink.SetId(linkId?.ToString() ?? "");
                  contentLink.OnCreated();
                  return contentLink;
               case ArrayDict referenceDict when typeof(IContentRef).IsAssignableFrom(type):
                  contentRef = (IContentRef) Activator.CreateInstance(type);
                  object id = "";
                  referenceDict.TryGetValue("id", out id);
                  contentRef.SetId(id?.ToString() ?? "");
                  return contentRef;
               case string linkString when typeof(IContentLink).IsAssignableFrom(type):
                  contentLink = (IContentLink) Activator.CreateInstance(type);
                  contentLink.SetId(linkString ?? "");
                  contentLink.OnCreated();
                  return contentLink;
               case string refString when typeof(IContentRef).IsAssignableFrom(type):
                  contentRef = (IContentRef) Activator.CreateInstance(type);
                  contentRef.SetId(refString ?? "");
                  return contentRef;

               /* PRIMITIVES TYPES */
               case string enumValue when typeof(Enum).IsAssignableFrom(type):
                  return Enum.Parse(type, enumValue);
               case string _:
                  return json.Substring(1, json.Length - 2);
               case float _:
                  return Convert.ChangeType(float.Parse(json, CultureInfo.InvariantCulture), type);
               case long _:
                  return Convert.ChangeType(long.Parse(json, CultureInfo.InvariantCulture), type);
               case double _:
                  return Convert.ChangeType(double.Parse(json, CultureInfo.InvariantCulture), type);
               case bool _:
                  return Convert.ChangeType(bool.Parse(json), type);
               case int _:
                  return Convert.ChangeType(int.Parse(json, CultureInfo.InvariantCulture), type);


               case ArrayDict dictionary when typeof(IDictionaryWithValue).IsAssignableFrom(type):
                  var dictInst = (IDictionaryWithValue) Activator.CreateInstance(type);

                  foreach (var kvp in dictionary)
                  {
                     var convertedValue = DeserializeResult(kvp.Value, dictInst.ValueType);
                     dictInst.Add(kvp.Key, convertedValue);
                  }

                  return dictInst;
               case IList list when type.IsArray:
                  var output = (IList) Activator.CreateInstance(type, new object[] {list.Count});
                  var fieldType = type.GetElementType();
                  for (var index = 0; index < list.Count; index++)
                  {
                     output[index] = DeserializeResult(list[index], fieldType);
                  }

                  return output;

               case IList list when TryGetElementType(list, type, out var listElementType):

                  var countConstructor = type.GetConstructor(new[] {typeof(int)});
                  var hasCountConstructor = countConstructor != null;

                  var listInstance = hasCountConstructor
                     ? Activator.CreateInstance(type, list.Count)
                     : Activator.CreateInstance(type);
                  var outputList = (IList) listInstance;
                  if (list.Count > 0 && listElementType == null)
                     throw new Exception($"Unable to deserialize list element type. {type}");
                  foreach (var elem in list)
                  {
                     var elemValue = DeserializeResult(elem, listElementType);
                     outputList.Add(elemValue);
                  }

                  return outputList;





               case ArrayDict assetDict when typeof(AssetReference).IsAssignableFrom(type):
                  object guid = "";
                  assetDict.TryGetValue("referenceKey", out guid);
                  var assetRef = (AssetReference) Activator.CreateInstance(type, guid);
                  if (assetDict.TryGetValue("subObjectName", out var subKey))
                  {
                     assetRef.SubObjectName = subKey.ToString();
                  }

                  return assetRef;


               case ArrayDict dict:

                  var fields = GetFieldInfos(type);
                  var instance = Activator.CreateInstance(type);
                  foreach (var field in fields)
                  {
                     object fieldValue = null;
                     if (dict.TryGetValue(field.SerializedName, out var property))
                     {
                        fieldValue = DeserializeResult(property, field.FieldType);
                     }
                     else
                     {
                        // check for the formerly serialized options...
                        var foundFormerly = false;
                        for (var i = 0; i < field.FormerlySerializedAs.Length; i++)
                        {
                           if (dict.TryGetValue(field.FormerlySerializedAs[i], out property))
                           {
                              // we found the field!!!
                              foundFormerly = true;
                              fieldValue = DeserializeResult(property, field.FieldType);
                              break;
                           }
                        }

                        if (!foundFormerly)
                        {
                           fieldValue = DeserializeResult(null, field.FieldType);
                        }
                     }

                     field.SetValue(instance, fieldValue);

                  }

                  return instance;
               default:
                  throw new Exception($"Cannot deserialize type [{type.Name}]");
            }
         }
         catch (Exception ex)
         {
            Debug.LogError($"Failed to deserialize field. type=[{type.Name}] data=[{preParsedValue}]");
            throw ex;
         }
      }
      private List<FieldInfoWrapper> GetFieldInfos(Type type)
      {
         FieldInfoWrapper CreateFieldWrapper(FieldInfo field)
         {
            var wrapper = new FieldInfoWrapper();
            var attr = field.GetCustomAttribute<ContentFieldAttribute>();
            if (attr != null && !string.IsNullOrEmpty(attr.SerializedName))
            {
               wrapper.SerializedName = attr.SerializedName;
            }
            else
            {
               wrapper.SerializedName = field.Name;
            }

            if (attr != null && attr.FormerlySerializedAs != null)
            {
               wrapper.FormerlySerializedAs = attr.FormerlySerializedAs;
            }
            else
            {
               wrapper.FormerlySerializedAs = new string[] { };
            }

            wrapper.RawField = field;

            return wrapper;
         }

         List<FieldInfo> GetAllPrivateFields(Type currentType)
         {
            // base case.
            var isNull = currentType == null;
            var isObjectType = currentType == typeof(System.Object);
            var isScriptableObjectType = currentType == typeof(ScriptableObject);
            var isContentObject = currentType == typeof(ContentObject) ;

            // XXX: Revisit this check when we allow customers to only implement IContentObject instead of subclass ContentObject
            var isCustomContentObject = currentType.BaseType == typeof(System.Object) &&
                                        currentType.GetInterfaces().Contains(typeof(IContentObject));
            if (isNull || isObjectType || isScriptableObjectType || isContentObject || isCustomContentObject)
            {
               return new List<FieldInfo>();
            }

            // private fields are only available via reflection on the target type, and any base type fields will need to be gathered by manually walking the type tree.
            var privateFields = currentType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
               .ToList();
            privateFields.AddRange(GetAllPrivateFields(currentType.BaseType));
            return privateFields;
         }

         var listOfPublicFields = type.GetFields(BindingFlags.Public | BindingFlags.Instance).ToList();
         var listOfPrivateFields = GetAllPrivateFields(type).Where(field =>
         {
            return field.GetCustomAttributes<SerializeField>() != null || field.GetCustomAttribute<ContentFieldAttribute>() != null;
         });

         var serializableFields = listOfPublicFields.Union(listOfPrivateFields);
         var notIgnoredFields = serializableFields.Where(field => field.GetCustomAttribute<IgnoreContentFieldAttribute>() == null);

         return notIgnoredFields.Select(CreateFieldWrapper).ToList();
      }

      [System.Serializable]
      public class PropertyValue : IRawJsonProvider
      {
         public string rawJson;

         public string ToJson()
         {
            return rawJson;
         }
      }

      /// <summary>
      /// returns only the {} representing the properties object
      /// </summary>
      /// <param name="content"></param>
      /// <typeparam name="TContent"></typeparam>
      /// <returns></returns>
      public string SerializeProperties<TContent>(TContent content)
         where TContent : IContentObject

      {
         var fields = GetFieldInfos(content.GetType())
            .ToDictionary(f => f.SerializedName);
         var propertyDict = new ArrayDict();

         foreach (var kvp in fields)
         {
            var fieldName = kvp.Key;
            var fieldInfo = kvp.Value;
            var fieldType = kvp.Value.RawField.FieldType;
            var fieldValue = fieldInfo.RawField.GetValue(content);
            var fieldDict = new ArrayDict();

            switch (fieldValue)
            {
               case IList list when
                  (list.GetType().GetGenericArguments().Length == 1 && typeof(IContentLink).IsAssignableFrom(list.GetType().GetGenericArguments()[0])) ||
                  (list.GetType().IsArray && typeof(IContentLink).IsAssignableFrom(list.GetType().GetElementType())):
                  var linkSet = new string[list.Count];
                  for (var i = 0; i < list.Count; i++)
                  {
                     var link = (IContentLink)list[i];
                     linkSet[i] = link.GetId();
                  }
                  fieldDict.Add("$links", linkSet);
                  propertyDict.Add(fieldName, fieldDict);
                  break;

               case IContentLink link:
                  fieldDict.Add("$link", link.GetId());
                  propertyDict.Add(fieldName, fieldDict);
                  break;
               default: // data block.
                  if (fieldValue is Optional optional)
                  {
                     if (optional.HasValue)
                     {
                        fieldValue = optional.GetValue();
                        fieldType = optional.GetOptionalType();
                     }
                     else
                     {
                        continue;
                     }
                  }
                  var jsonValue = SerializeArgument(fieldValue, fieldType);
                  fieldDict.Add("data", new PropertyValue { rawJson = jsonValue });
                  propertyDict.Add(fieldName, fieldDict);
                  break;
            }
         }


         var json = Json.Serialize(propertyDict, new StringBuilder());
         return json;
      }

      /// <summary>
      /// Returns the {id: 1, version: 1, properties: {}} json model.
      /// </summary>
      /// <param name="content"></param>
      /// <typeparam name="TContent"></typeparam>
      /// <returns></returns>
      public string Serialize<TContent>(TContent content)
         where TContent : IContentObject, new()
      {
         var contentDict = new ArrayDict
         {
            {"id", content.Id},
            {"version", content.Version ?? ""}
         };

         var propertyDict = new PropertyValue { rawJson = SerializeProperties(content) };
         contentDict.Add("properties", propertyDict);

         var json = Json.Serialize(contentDict, new StringBuilder());
         return json;
      }


      protected abstract TContent CreateInstance<TContent>() where TContent : TContentBase, IContentObject, new();
      public TContentBase DeserializeByType(string json, Type contentType)
      {
         return (TContentBase)GetType()
            .GetMethod(nameof(Deserialize))
            .MakeGenericMethod(contentType)
            .Invoke(this, new[] { json });
      }
      public TContent Deserialize<TContent>(string json)
         where TContent : TContentBase, IContentObject, new()
      {
         var instance = CreateInstance<TContent>();

         var fields = GetFieldInfos(typeof(TContent));
         var deserializedResult = Json.Deserialize(json);
         var root = deserializedResult as ArrayDict;

         if (root == null) throw new ContentDeserializationException(json);
         var id = root["id"].ToString();

         // the id may be a former name. We should always prefer to use the latest name based on the actual type of data being deserialized.
         var typeName = "";

         var type = ContentRegistry.GetTypeFromId(id);
         if (!ContentRegistry.TryGetName(type, out typeName))
         {
            typeName = ContentRegistry.GetTypeNameFromId(id);
         }

         var name = ContentRegistry.GetContentNameFromId(id);
         id = string.Join(".", typeName, name);

         var version = root["version"];

         var properties = root["properties"] as ArrayDict;
         instance.SetIdAndVersion(id, version.ToString());


         foreach (var field in fields)
         {
            var fieldName = field.SerializedName;
            if (!properties.TryGetValue(fieldName, out var property))
            {
               // mark empty optional, if exists.
               if (typeof(Optional).IsAssignableFrom(field.FieldType))
               {
                  var optional = Activator.CreateInstance(field.FieldType);
                  field.SetValue(instance, optional);
               }

               // no property exists for this field. Maybe we should check the formerly known fields.
               var foundFormerlySerializedAs = false;
               for (var i = 0; i < field.FormerlySerializedAs.Length; i++)
               {
                  if (properties.TryGetValue(field.FormerlySerializedAs[i], out property))
                  {
                     // ah ha! We found the field...
                     foundFormerlySerializedAs = true;
                     field.SerializedName = field.FormerlySerializedAs[i];
                     break;
                  }
               }

               if (!foundFormerlySerializedAs)
               {
                  continue; // there is no property for this field...

               }
            }

            if (property is ArrayDict propertyDict)
            {
               if (propertyDict.TryGetValue("data", out var dataValue))
               {
                  var hackResult = DeserializeResult(dataValue, field.FieldType);
                  field.SetValue(instance, hackResult);
               }

               if (propertyDict.TryGetValue("$link", out var linkValue) || propertyDict.TryGetValue("link", out linkValue))
               {
                  if (!typeof(IContentLink).IsAssignableFrom(field.FieldType))
                  {
                     throw new Exception($"Cannot deserialize a link into a field that isnt a link field=[{field.SerializedName}] type=[{field.FieldType}]");
                  }

                  var link = (IContentLink)Activator.CreateInstance(field.FieldType);
                  link.SetId(linkValue.ToString());
                  link.OnCreated();
                  field.SetValue(instance, link);
               }

               if (propertyDict.TryGetValue("$links", out var linksValue) ||
                   propertyDict.TryGetValue("links", out linksValue))
               {

                  var set = (IList<object>)linksValue;
                  var links = Activator.CreateInstance(field.FieldType, set.Count);

                  var linkList = (IList)links;
                  Type elemType;
                  if (field.FieldType.IsArray)
                  {
                     elemType = field.FieldType.GetElementType();
                  }
                  else if (field.FieldType.GenericTypeArguments.Length == 1)
                  {
                     elemType = field.FieldType.GenericTypeArguments[0];
                  }
                  else
                  {
                     throw new Exception("Unknown link list type");
                  }

                  for (var i = 0; i < set.Count; i++)
                  {
                     var elem = (IContentLink)Activator.CreateInstance(elemType);
                     elem.SetId(set[i].ToString());
                     elem.OnCreated();

                     if (linkList.Count <= i)
                     {
                        linkList.Add(elem);
                     }
                     else
                     {
                        linkList[i] = elem;

                     }
                  }

                  field.SetValue(instance, links);

               }
            }
         }


         return instance;
      }
   }
}