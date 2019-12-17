using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using MongoDB.Bson;

namespace Kugar.Tool.MongoDBHelper.Validators
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter,
        AllowMultiple = false)]
    public class NotEmptyAttribute:ValidationAttribute
    {
        public NotEmptyAttribute() : base("{0}不能为空") { }

        public override bool IsValid(object value)
        {
            if (value ==null)
            {
                return false;
            }
            else if (!(value is ObjectId))
            {
                return false;
            }
            else if ((ObjectId)value == ObjectId.Empty)
            {
                return false;
            }

            return true;
        }

    }
}
