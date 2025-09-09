# iAM Simulation Algorithm Framework (SAF)



## overall remarks

- The simulation of iAM (formerly RoadCare) is an ARA algorithm framework (not
  code in itself, but still having an original reference implementation in the
  RoadCare desktop application). In contrast, "AssetFox" is a specific PennDOT
  web application that contains one implementation of that framework.
- Certain aspects of AssetFox's implementation of the iAM simulation are not
  part of the iAM simulation framework. For example, "calculated field timing"
  is an AssetFox-specific item introduced by PennDOT into their implementation
  of the iAM simulation. Another example is "performance curve adjustment
  factors".

## plan of writing

- We want something that's both high-level and detailed. We want to know why
  things like "calculated field timing" exist (answer to that one: no idea), but
  we don't necessarily care about describing how exactly that thing gets handled
  at the code/book-keeping level.
- Also, note we're distinguishing the overall framework algorithm from the
  yearly decision-making (project-picking) algorithm. Each has its own
  required/suppplemental inputs and outputs.

## sketch of content

### inputs

- To the simulation, there is no network or "explorer". It's just a set of
  assets and a set of attributes.
- assets
  - ID, name, description, whatever
  - list of attribute-value pairs
- attribute
  - either number (double) or text (string)
    - number type should probably be decimal, but it doesn't matter for the
      sake of this doc.
  - also, "calculated fields". Probably should be called "calculated
    attributes". But "calculation" is too general at the conceptual level, even
    though it has a specific meaning re CalculateEvaluate within the system.
  - min, max
    - no "default" within the simulation, as it's only used for attribute
      value initialization.
  - deterioration/performance curves
    - target attribute (number only; not calculated)
    - criterion for an asset to satisfy in order for this curve to apply to
      the target attribute
    - the curve itself is either
      - an interpolated function of special automatic "age" attribute, or
      - an "equation" expression that's a function of some subset of the asset's
        current attributes; this dependency subset can contain the curve's
        affected attribute, but it should not contain any other attribute which
        is (or depends on) an attribute subject to deterioration.
- treatment
  - consequences
  - cost
  - list of budgets that are allowed to fund this treatment
- budget
  - amount of new money per time step
  - list of criteria that an asset must satisfy in order to receive a
    treatment with funding from this budget
- cash-flow rules
- initial attribute values on each asset
  - This is fundamentally **input**, not necessarily a part of simulation logic.
    Currently, the simulation handles the calculation of initial attribute
    values based on historical attribute values, but it should be decoupled to a
    preprocessing step.
- algorithm-specific metadata, like budget priority levels, budget order, etc.
  - These things could be discarded if the algorithm changed. Other inputs
    cannot be discarded; they are essential to achieving the outputs.
- time step definition (traditionally, "year", but the time step is handled
  very abstractly)
- objective function
- condition goals
- spending strategy
- other settings:
  - yearly budget remainder allowed to roll over to next year?
  - multiple budgets allowed to fund a single treatment?
  - assets allowed to deteriorate during cash-flow?
  - assets grouped for treatment selection?

### flow per time step

- start with last step's "final values"
- DETERIORATION
  - carries the attributes into the current phase of decision-making
  - previously .
  - an asset or its attributes may not deteriorate under certain conditions,
    e.g. active cash-flow.
- determine project options, including null project options based on the null
  treatment
  - check treatment feasibility criteria, shadows, etc
  - could also consider 2 different cash flow periods as 2 different project
    options
- estimate the "objective value" of each project option
  - PLUG-IN: needs an input/output spec
- determine selected projects (i.e. decide on funding, e.g. budgets, cash-flow?)
  - PLUG-IN: needs an input/output spec
- PROJECT EFFECTS
- done; next time step

### outputs

- a project activity schedule that shows what treatment decisions were made when
  and what the effects of those decisions were (on the attribute values of
  each asset)
  - each year, you're not really "generating a new schedule for that year",
    you're more "filling in more of the overall simulation period schedule
    based on that year's data".
- "reasons" why each decision was made
  - This would be completely obviated by using CP for the decision phase of
    each year.
  - This is an example of accidental output, specific to the "picker"
    algorithm.
- "what output" vs "why output"
  - core output is what-output, picker-specific output is why-output

### misc remarks

- treatment bundling should be implicitly performed by a CP abiding by
  supersede/concurrency rules. it should not be handled by the framework.
- benefit/cost should reflect total cost of all "follow-up" (i.e. scheduled)
  treatments
- the quasi-required "age" attribute is not technically required. It's only used
  to facilitate handling of piecewise expressions as performance curves. In
  other words, if that kind of "special" expression could include the dependency
  attribute in its specification, there's no need to have "special" attributes.
- alternative methods of defining functions may exist, e.g. "piecewise
  expressions" as used by RoadCare and AssetFox.

### some potential new terms

- project: the combination of a treatment and an asset (i.e. the application of
  a treatment to an asset) over a certain time period (one or more time steps).
  - project activity: one time step of a project
  - null project: a single-step project whose treatment is the null treatment.
  - project option
  - selected project
  - committed project: defined by the user and by projects' treatments'
    required future treatments

## draft of content

### Overview

- _Scenario_ | _Simulation_ | _Project Plan_

### Document Glossary

- _Inputs/Outputs/Aspects (IOA)_

### Framework Glossary


### Variable implementation details

- how to handle multiple "true" predicates when evaluating a CFS
  - e.g. abort the simulation; evaluate all the corresponding functions and take
    the best or worst value
- how to handle zero "true" predicates when evaluating a CFS with no default
  - e.g. abort the simulation; if evaluating deterioration, fall back to a
    "default value" on that attribute's definition
- "default values" in numeric attribute definitions
  - generally, only useful in initial total snapshot preparation when historical
    data is incomplete; but, could be used in other implementation-specific
    situations

### RoadCare-specific IOA

- "assets" are "segments" or "sections"

### RoadCare- and AssetFox-specific IOA

- "direction of deterioration"? could it be an implementation detail?
- "default value" on each attribute definition
- "AGE" attribute
- age-dependent "piecewise" expressions for interpolated functions in
  deterioration models
- "value-change" expressions in treatment consequences
- budget priority levels
- budget order

### AssetFox-specific IOA

- performance curve adjustment factors
- calculated field timing

### Glossary of Terms & Structured Data

Italicized text is used to indicate parts of a data structure. When such text is
followed by a colon, the text after the colon clarifies the general type of data
in the named part. If the part is optional, a question mark is shown.

- **Simulation Time Sequence**
  - This is the sequence of abstract time steps and sub-steps over which the
    simulation operates.
- **Asset**
  - This is any object that (a) may deteriorate over time and (b) may be managed
    or maintained by applying treatments to the object.
  - Within the simulation, an asset is nothing more than its identity.
- **Attribute Definition**
  - This describes a property associated with some or all assets in the
    simulation.
- **Asset Snapshot**
  - This is a set of values, one for each attribute definition, fully describing
    a single asset at a single moment in the simulation time sequence.
- **Total Snapshot**
  - This is a set of asset snapshots, one for each asset in the scenario, for a
    single step in the simulation time sequence.
- **Asset Numeric Function**
  - This is a function that applies to an asset snapshot and evaluates to a real
    number.
- **Asset Textual Function**
  - This is a function that applies to an asset snapshot and evaluates to a text
    string.
- **Asset Predicate**
  - This is a logical predicate that applies to an asset snapshot and evaluates
    to a classical truth value (i.e. either "true" or "false").
- **Conditional Function System (CFS)**
  - This is a "default" asset function alongside a set of asset
    predicate-function pairs.
  - The default function is optional only if there is at least one
    predicate-function pair.
  - The asset functions in a single CFS must be either all numeric or all
    textual.
- **Numeric Attribute Definition**
  - _Minimum Value_ : number (?)
  - _Maximum Value_ : number (?)
  - _Deterioration Model_ : numeric CFS (?)
    - _Direction of Deterioration_ : negative or positive
- **Textual Attribute Definition**
- **Numeric Parameter Definition**
  - _Value Model_ : CFS
- **Attribute Value Effect**
  - _Target Attribute_
  - _Effect Model_ : CFS
  - The target attribute and the effect model must match value-types. In other
    words, the target is a numeric attribute and the effect model uses numeric
    functions, or the target is a textual attribute and the effect model uses
    textual functions; other combinations are invalid.
- **Null Treatment**
  - _Attribute Value Effects_ : (*)
- **Selectable Treatment**
  - _Attribute Value Effects_ : (+)
  - _Cost Model_ : CFS
  - _Selection Predicate_ : (?)
  - _Allowed Budgets_ : (+)
  - _Required Future Treatments_ : (*)
- **Scenario**
  - This is a container of all input required for a single execution of the
    simulation algorithm.
  - _Initial Total Snapshot_
  - _Numeric Attribute Definitions_
  - _Numeric Parameter Definitions_
  - _Null Treatment_
  - _Selectable Treatments_
    - Required Future Treatments; each:
      - Future Offset
      - Required Treatment
  - _Allowed Treatment Concurrencies_; each:
    - Concurrently Allowed Treatments

### Output Data

- _Project Plan_

### Data Operations

- _Simulation_