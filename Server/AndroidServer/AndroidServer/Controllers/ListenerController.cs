using AndroidServer.Domain;
using AndroidServer.Domain.Listeners;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AndroidServer.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ListenerController : ControllerBase
    {
        private readonly AndroidService android;

        public ListenerController(AndroidService android)
        {
            this.android = android;
        }

        [HttpGet("types")]
        public IEnumerable<ListenerType> GetListenerTypes()
        {
            return ListenerTypes.GetListenerTypes();
        }

        [HttpGet("ui")]
        public ListenerUIInfo GetListenerUI(string typeID)
        {
            var type = ListenerTypes.GetRawType(typeID);
            if (type == null)
                return null;

            bool hideBaseMembers = type.GetCustomAttribute<HideListenerBaseAttribute>() != null;

            var ui = new ListenerUIInfo();

            foreach (var property in type.GetProperties())
            {
                var attribute = property.GetCustomAttribute<UiVariableTypeAttribute>(true);
                if (attribute == null)
                    continue;

                var entry = new ListenerUIEntry
                {
                    Name = property.Name,
                    Type = attribute.VariableType
                };

                if (property.DeclaringType == type)
                    ui.Variables.Add(entry);
                else if (!hideBaseMembers)
                {
                    ui.Variables.Insert(0, entry);
                }
            }

            return ui;
        }

        [HttpPost("setting")]
        public IActionResult SetListenerSetting(string id, string propertyName, object value)
        {
            var listener = android.AndroidInstances.SelectMany(a => a.Value.Listeners).FirstOrDefault(a => a.ID == id);
            if (listener == null)
                return NotFound();

            var type = listener.GetType();
            var property = type.GetProperty(propertyName);
            if (property == null)
                return NotFound();

            var attr = property.GetCustomAttribute<UiVariableTypeAttribute>();

            try
            {
                var obj = (JObject)value; ;
                var token = obj.GetValue("data");

                switch (attr.VariableType)
                {
                    case VariableType.String:
                    case VariableType.TextChannel:
                    case VariableType.VoiceChannel:
                    case VariableType.TextArea:
                    case VariableType.RoleID:
                    case VariableType.EmoteID:
                        {
                            var s = token.ToObject<string>();
                            property.SetValue(listener, s);
                            listener.OnPropertyChange(propertyName, s);
                        }
                        break;
                    case VariableType.Number:
                        {
                            float f = token.ToObject<float>();
                            property.SetValue(listener, f);
                            listener.OnPropertyChange(propertyName, f);
                        }
                        break;
                    case VariableType.Boolean:
                        {
                            bool b = token.ToObject<bool>();
                            property.SetValue(listener, b);
                            listener.OnPropertyChange(propertyName, b);
                        }
                        break;
                    default:
                        {
                            property.SetValue(listener, value);
                            listener.OnPropertyChange(propertyName, value);
                        }
                        break;
                }
            }
            catch (Exception)
            {
                return BadRequest();
            }

            return Ok();
        }

        [HttpGet("setting")]
        public object GetListenerSetting(string id, string propertyName)
        {
            var listener = android.FindListener(id);
            if (listener == null)
                return null;

            var type = listener.GetType();
            var property = type.GetProperty(propertyName);
            if (property == null)
                return null;

            var result = property.GetValue(listener);

            return new
            {
                data = result
            };
        }

        [HttpGet]
        public ListenerInfo GetListener(string id)
        {
            var found = android.FindListener(id);
            if (found == null)
                return null;

            return ListenerInfo.Create(found);
        }

        [HttpPost("active")]
        public void SetListenerActive(string id, bool active)
        {
            var found = android.FindListener(id);
            if (found == null)
                return;

            found.Active = active;
        }

        [HttpGet("filter")]
        public ResponseFilters GetListenerFilter(string id)
        {
            var found = android.FindListener(id);
            if (found == null)
                return null;

            return found.ResponseFilters;
        }

        [HttpPost("filter")]
        public void SetListenerFilter(string id, [FromBody] ResponseFilters filters)
        {
            var found = android.FindListener(id);
            if (found == null)
                return;

            found.ResponseFilters = filters ?? new ResponseFilters();

            found.ResponseFilters.Roles = found.ResponseFilters.Roles.Where(r => r != null).ToList();
            found.ResponseFilters.Users = found.ResponseFilters.Users.Where(r => r != null).ToList();
        }
    }
}
