using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IOWebApplication.Core.Helper
{
    public static class JsonUtils
    {
        public static void ParseJsonProperties(JObject jObject, string paramName, Dictionary<string, string> jsonKeyValues, bool firstObj)
        {
            IEnumerable<JProperty> jObject_Properties = jObject.Properties();

            // Build list of valid property and object types 
            JTokenType[] validPropertyValueTypes = { JTokenType.String, JTokenType.Integer, JTokenType.Float, JTokenType.Boolean, JTokenType.Null, JTokenType.Date, JTokenType.Bytes, JTokenType.Guid, JTokenType.Uri, JTokenType.TimeSpan };
            List<JTokenType> propertyTypes = new List<JTokenType>(validPropertyValueTypes);

            JTokenType[] validObjectTypes = { JTokenType.String, JTokenType.Array, JTokenType.Object };
            List<JTokenType> objectTypes = new List<JTokenType>(validObjectTypes);

            string currentParamName = paramName; //Need to track where we are.

            foreach (JProperty property in jObject_Properties)
            {
                paramName = currentParamName;

                try
                {
                    if (propertyTypes.Contains(property.Value.Type))
                    {
                        ParseJsonKeyValue(property, paramName + "_" + property.Name.ToString(), jsonKeyValues);
                    }
                    else if (objectTypes.Contains(property.Value.Type))
                    {
                        //Arrays ex. { names: ["first": "John", "last" : "doe"]}
                        if (property.Value.Type == JTokenType.Array && property.Value.HasValues)
                        {
                            ParseJsonArray(property, paramName, jsonKeyValues, firstObj);
                        }

                        //Objects ex. { name: "john"}
                        if (property.Value.Type == JTokenType.Object)
                        {
                            JObject jo = new JObject();
                            jo = JObject.Parse(property.Value.ToString());

                            //На първият обект да не слага името
                            if (firstObj == false)
                                paramName = paramName + "_" + property.Name.ToString();

                            //без добавяне на главния елемент
                            //jsonKeyValues.Add(paramName, property.Value.ToString());

                            if (jo.HasValues)
                            {
                                ParseJsonProperties(jo, paramName, jsonKeyValues, false);
                            }

                        }
                    }
                }
                catch (Exception ex)
                {
                    throw;
                }
            } // End of ForEach

            paramName = currentParamName;

        }

        public static void ParseJsonKeyValue(JProperty item, string paramName, Dictionary<string, string> jsonKeyValues)
        {
            jsonKeyValues.Add(paramName, item.Value.ToString());
        }

        public static void ParseJsonArray(JProperty item, string paramName, Dictionary<string, string> jsonKeyValues, bool firstObj)
        {
            JArray jArray = (JArray)item.Value;

            paramName = paramName + "_" + item.Name.ToString();
            jsonKeyValues.Add(paramName, item.Value.ToString());

            string currentParamName = paramName; //Need track where we are

            try
            {
                for (int i = 0; i < jArray.Count; i++)
                {
                    paramName = currentParamName;

                    paramName = paramName + "_" + i.ToString();
                    jsonKeyValues.Add(paramName, jArray.Values().ElementAt(i).ToString());

                    JObject jo = new JObject();
                    jo = JObject.Parse(jArray[i].ToString());
                    IEnumerable<JProperty> jArrayEnum = jo.Properties();

                    foreach (JProperty jaItem in jArrayEnum)
                    {
                        // Prior to JSON.NET VER 5.0, there was no Path property on JTokens. So we had to track the path on our own.
                        var paramNameWithJaItem = paramName + "_" + jaItem.Name.ToString();

                        var itemValue = jaItem.Value.ToString(Newtonsoft.Json.Formatting.None);
                        if (itemValue.Length > 0)
                        {
                            switch (itemValue.Substring(0, 1))
                            {
                                case "[":
                                    //Recusion call to itself
                                    ParseJsonArray(jaItem, paramNameWithJaItem, jsonKeyValues, firstObj);
                                    break;
                                case "{":
                                    //Create a new JObject and parse
                                    JObject joObject = new JObject();
                                    joObject = JObject.Parse(itemValue);

                                    //For this value, reparse from the top
                                    ParseJsonProperties(joObject, paramNameWithJaItem, jsonKeyValues, firstObj);
                                    break;
                                default:
                                    ParseJsonKeyValue(jaItem, paramNameWithJaItem, jsonKeyValues);
                                    break;
                            }
                        }
                    }
                } //end for loop

                paramName = currentParamName;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

    }
}
