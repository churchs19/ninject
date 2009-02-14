﻿#region License
// Author: Nate Kohari <nkohari@gmail.com>
// Copyright (c) 2007-2009, Enkari, Ltd.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//   http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion
#region Using Directives
using System;
using Ninject.Activation;
#endregion

namespace Ninject.Parameters
{
	/// <summary>
	/// Modifies an activation process in some way.
	/// </summary>
	public class Parameter : IParameter
	{
		/// <summary>
		/// Gets the name of the parameter.
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		/// Gets or sets the callback that will be triggered to get the parameter's value.
		/// </summary>
		public Func<IContext, object> ValueCallback { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="Parameter"/> class.
		/// </summary>
		/// <param name="name">The name of the parameter.</param>
		/// <param name="value">The value of the parameter.</param>
		public Parameter(string name, object value) : this(name, ctx => value) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="Parameter"/> class.
		/// </summary>
		/// <param name="name">The name of the parameter.</param>
		/// <param name="valueCallback">The callback that will be triggered to get the parameter's value.</param>
		public Parameter(string name, Func<IContext, object> valueCallback)
		{
			Name = name;
			ValueCallback = valueCallback;
		}

		/// <summary>
		/// Gets the value for the parameter within the specified context.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <returns>The value for the parameter.</returns>
		public object GetValue(IContext context)
		{
			return ValueCallback(context);
		}

		/// <summary>
		/// Determines whether the object equals the specified object.
		/// </summary>
		/// <param name="obj">An object to compare with this object.</param>
		/// <returns><c>True</c> if the objects are equal; otherwise <c>false</c></returns>
		public override bool Equals(object obj)
		{
			var parameter = obj as IParameter;
			return parameter != null ? Equals(parameter) : base.Equals(obj);
		}

		/// <summary>
		/// Serves as a hash function for a particular type.
		/// </summary>
		/// <returns>A hash code for the object.</returns>
		public override int GetHashCode()
		{
			return GetType().GetHashCode() ^ Name.GetHashCode();
		}

		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns><c>True</c> if the objects are equal; otherwise <c>false</c></returns>
		public bool Equals(IParameter other)
		{
			return other.GetType() == GetType() && other.Name.Equals(Name);
		}
	}
}