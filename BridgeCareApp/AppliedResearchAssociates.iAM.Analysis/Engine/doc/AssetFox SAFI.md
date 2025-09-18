# AssetFox Implementation of iAM SAF

---

# sketch/draft content

- amazingly, I think the null treatment (i.e. "no treatment") is an
  implementation detail. Makes sense, given that conceptually it has no
  real-world analogue other than deterioriation, which is where its effects
  should incorporated.

### misc remarks

- initial attribute values on each asset
  - This is fundamentally **input**, not necessarily a part of simulation logic.
    Currently, the simulation handles the calculation of initial attribute
    values based on historical attribute values, but it should be decoupled to a
    preprocessing step.
- SAFI-specific metadata, like budget priority levels, budget order, etc.
- treatment bundling should be implicitly performed by a CP abiding by
  supersede/concurrency rules.
- benefit/cost could reflect total of all "follow-up" (i.e. scheduled)
  treatments, but it's up to the SAFI.
- the quasi-required "age" attribute is not technically required. It's only used
  to facilitate handling of piecewise expressions as performance curves. In
  other words, if that kind of "special" expression could include the dependency
  attribute(s) in its specification, there's no need to have "special"
  attributes.
- alternative methods of defining functions may exist, e.g. "piecewise
  expressions" as used by RoadCare and AssetFox.

### Variable implementation details

- "default values" in numeric attribute definitions
  - generally, only useful in initial total snapshot preparation when historical
    data is incomplete; but, could be used in other implementation-specific
    situations
- EXTENSIVE "decision rationale" output. Adds lots of complexity, but the
  primary issue with it is that it's intermixed with the project selection
  output, which bloats the latter significantly.

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
