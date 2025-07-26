## Smithbox to Paramdex

This repo contains a small, simple script to convert between [Smithbox]'s
JSON-based format for parameter names and [Paramdex]'s text-based format and
vice-versa.

[Smithbox]: https://github.com/vawser/smithbox
[Paramdex]: https://github.com/soulsmods/Paramdex

```
dotnet run Program.cs -- path/to/Smithbox path/to/Paramdex
```

### Why convert?

It's valuable to have a single, canonical source of truth for parameter names.
As various FromSoft modders datamine these games, we each accumulate knowledge
about how bits and pieces work and what each speffect and entity ID represents.
A place to share this knowledge gives us the means to build on one another's
work and create a community base of understanding greater than what any of us
can hold in our heads individually.

Both Smithbox and Paramdex have been used, at various times by various people,
as sources of truth for inspecting and modifying these games. With Smithbox
recently being briefly archived due to maintainer burnout, it became clear that
it would be valuable to ensure its data was portable. With it being un-archived,
it became valuable in turn to take the work people had put into Paramdex and
roll it back into the more graphical tool.

So, this script exists as a means of converting back and forth for as long as
both sources are in wide use. It's my hope that eventually the community settles
on and users of the other migrate over, but until that point, liquidity of
information between them is the best alternative.

### Options

* `--paramdex-to-smithbox`: By default this tool converts Smithbox's data into
  Paramdex's format. This reverses the flow, and converts Paramdex's data into
  Smithbox's format.

* `--overwrite-names`: By default, if the target repository already has a name
  for a given parameter, that name is preserved even if it differs from the
  source. If this flag is passed, all target names are overwritten (as long as
  the source has a name).

* `--game`: Takes a game code, like `ER` for _Elden Ring_ or `AC6` for _Armored
  Core 6_. Can be passed multiple times. Only parameters for the given game(s)
  are converted.
