using UnityEngine;
using System.Collections;



public interface ILinkable
{
    /// <summary>
    /// Model Name to which this Linkable should be Linked
    /// </summary>
    string modelName { get; set; }
    /// <summary>
    /// Establishes a link between this Linkable and given Model
    /// </summary>
    void Link( IModel model );
}
