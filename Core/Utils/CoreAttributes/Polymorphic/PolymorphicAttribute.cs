using System;
using UnityEngine;

public class PolymorphicAttribute : PropertyAttribute { }

// In the rare case where there is another attribute that rewrites the drawing of a property, but you want to
// allow the use of the polymorphic propertydrawer, skip drawing the content after the polymorphic dropdown to
// allow them to use ULElements.Property to draw this dropdown without also redrawing all of the properties fields
public class AttributeNeedsPolymorphicDrawerToIgnoreContentsButDrawDropdownAttribute : Attribute { }