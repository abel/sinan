using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinan.AMF3;

namespace Sinan.Data
{
    /// <summary>
    /// An interface defining a Thing.
    /// </summary>
    public interface IThing : IPersistable, IExternalizable
    {
        /// <summary>Gets or sets the ID of this thing.</summary>
        long Id { get; set; }

        /// <summary>Gets the name of this thing.</summary>
        string Name { get; }

        /// <summary>Gets the full name of this thing.</summary>
        string FullName { get; }

        /// <summary>Gets the description of this thing.</summary>
        string Description { get; }

        /// <summary>Gets or sets the title of this thing.</summary>
        string Title { get; set; }

        /// <summary>Gets or sets the parent of this thing, IE a container.</summary>
        IThing Parent { get; set; }
    }
}
