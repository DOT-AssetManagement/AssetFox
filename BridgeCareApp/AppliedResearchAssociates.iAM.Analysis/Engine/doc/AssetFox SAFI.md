# AssetFox Implementation of iAM SAF

---

# sketch/draft content

### Variable implementation details

- "default values" in numeric attribute definitions
  - generally, only useful in initial total snapshot preparation when historical
    data is incomplete; but, could be used in other implementation-specific
    situations

### Implementation details of RoadCare

- "assets" are "segments" or "sections"

### Implementation details of both RoadCare and AssetFox

- benefit??? seems like benefit attribute designation is also an implementation
  detail. If you only use remaining-life, then benefit never gets used.
- inflation rate handling
- numeric parameters ("calculated fields"), which only exist (a) to make textual
  expression construction more convenient and (b) to show "virtual attributes"
  in asset snapshots. These are not essential to the framework.
- textual attributes, which only exist (a) as additional data to operate on
  within asset predicates and (b) to show in asset snapshots. These are also not
  essential to the framework. They're an implementation detail.
- terminology mapping, e.g. numeric parameter definitions => "calculated fields"
- "direction of deterioration"? could it be an implementation detail?
- "default value" on each attribute definition
- "AGE" attribute
- age-dependent "piecewise" expressions for interpolated functions in
  deterioration models
- "value-change" expressions in treatment consequences
- budget priority levels
- budget order
- "outlook" algorithm and its configuration, e.g. length of time etc.
- "calculate-evaluate" expression language for user-friendly construction of
  asset functions and predicates
- benefit weighting, spatial weighting
- inflation rate percentage
- "until conditions met" strategies. they require linear selection strategies
  (like the greedy heuristic one currently in RoadCare/AssetFox).
  - can't be a framework level thing, as the framework should not impose
    requirements on the selection strategy of the implementations.

### Implementation details of AssetFox

- In AssetFox, cost models are (via a global option) allowed to be summing AFSs
- minimum project cost limit
- performance curve adjustment factors
- calculated field timing
- "pre-application" of the null treatment (just after deterioration, before
  project selection)
  - frankly, this looks like an awkward attempt to patch up incomplete
    deterioration rules
- minimum project cost limit
