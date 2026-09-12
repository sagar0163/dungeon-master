# Issue #4: Vertical Slice - Builder<->Crawl Loop on a Single Grid

## Checklist (ordered by dependency)

- [x] Create BuilderMode.tscn scene with grid floor, placement indicators, Essence UI
- [x] Wire BuilderController.cs to BuilderMode scene (export Camera3D, SelectionIndicator, PlacementPreview)
- [x] Create minimal ground plane and grid visual for Builder Mode (>= 8x8x1)
- [x] Create CrawlMode.tscn scene with CharacterBody3D camera, tile movement
- [x] Wire CrawlController.cs to CrawlMode scene (export Camera3D, CharacterBody3D)
- [x] Create GameRoot.tscn scene that manages mode switching between Builder and Crawl
- [ ] Implement mode switching: Tab hotkey toggles Builder<->Crawl in <100ms
- [ ] On Crawl start: Lord spawns at most recently built room
- [ ] Verify tile placed top-down is physically present first-person (single data model)
- [ ] Add GIF of build->switch->walk loop to README quick start
- [ ] Ensure dotnet build succeeds; no stale API references
- [ ] Run existing Python tests - all must pass

