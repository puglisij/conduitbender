using UnityEngine;
using System.Collections;

public interface ILinkableAnnouncer
{

    void AddLinker( ILinker linker );
    void RemoveLinker( ILinker linker );
    void Announce();
}
