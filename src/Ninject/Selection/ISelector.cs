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
using System.Collections.Generic;
using System.Reflection;
using Ninject.Components;
using Ninject.Selection.Heuristics;
#endregion

namespace Ninject.Selection
{
	/// <summary>
	/// Selects members for injection.
	/// </summary>
	public interface ISelector : INinjectComponent
	{
		/// <summary>
		/// Gets or sets the constructor scorer.
		/// </summary>
		IConstructorScorer ConstructorScorer { get; set; }

		/// <summary>
		/// Gets the property injection heuristics.
		/// </summary>
		ICollection<IPropertyInjectionHeuristic> PropertyInjectionHeuristics { get; }

		/// <summary>
		/// Gets the method injection heuristics.
		/// </summary>
		ICollection<IMethodInjectionHeuristic> MethodInjectionHeuristics { get; }

		/// <summary>
		/// Selects the constructor to call on the specified type, by using the constructor scorer.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <returns>The selected constructor, or <see langword="null"/> if none were available.</returns>
		ConstructorInfo SelectConstructor(Type type);

		/// <summary>
		/// Selects properties that should be injected, by using the property injection heuristics.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <returns>A series of the selected properties.</returns>
		IEnumerable<PropertyInfo> SelectPropertiesForInjection(Type type);

		/// <summary>
		/// Selects methods that should be injected, by using the method injection heuristics.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <returns>A series of the selected methods.</returns>
		IEnumerable<MethodInfo> SelectMethodsForInjection(Type type);
	}
}