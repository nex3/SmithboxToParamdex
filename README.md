## Smithbox to Paramdex

This repo contains a small, simple script to convert between [Smithbox]'s
JSON-based format for parameter names and [Paramdex]'s text-based format.

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

Until recently, Smithbox was the *de facto* source of truth, as the all-in-one
graphical tool for inspecting and modifying these games. However, as of 4 July
2025, the previous maintainer has archived it and no new maintainers have
stepped up. Since archiving renders it read-only, it's clearly no longer an
appropriate canonical source of truth.

Paramdex is the next best option. While not as directly connected to tooling as
Smithbox, it's actively used by at least [one prolific modder] and has existed
for a long time.

[one prolific modder]: https://thefifthmatt.com/

### Can we convert in reverse?

Currently this script only supports converting _from_ Smithbox _to_ Paramdex.
This does have the downside that any additions made solely in Paramdex can't be
viewed from within the last released Smithbox version (which should continue to
work indefinitely for existing FromSoft games). This makes graphical data mining
more difficult and error-prone.

It's possible to do a reverse conversion, but there's one substantial difficulty
standing in the way: Smithbox's parameter name format requires knowledge of the
indexes of each row, while Paramdex's does not. Aligning IDs with indices
accurately requires loading the game data, which is a much more complex task
than this package is currently capable of.

Support for the reverse transformation may be added in the future, especially if
Paramdex starts to have more substantial contributions that are missing in
Smithbox or any forks thereof. But for now, I'm keeping this simple and doing
the easier, higher-value translation.
