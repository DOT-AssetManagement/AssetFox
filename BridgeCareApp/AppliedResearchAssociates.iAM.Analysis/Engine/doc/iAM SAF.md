# iAM Simulation Algorithm Framework (SAF)

> The purpose of this document is not to describe any specific software
> application or any specific body of software code. The purpose is to describe
> a general algorithmic framework allowing various software implementations of
> one simulation-based approach to asset management project planning.

## overall remarks

- The simulation of iAM (formerly RoadCare) is an ARA algorithm framework (not
  code in itself, but still having an original reference implementation in the
  ARA RoadCare desktop application). In contrast, "AssetFox" is a specific
  PennDOT web application that contains one implementation of that framework.
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
  - e.g. consider it an input error, abort the simulation; evaluate all the
    corresponding functions and take the best or worst value
- how to handle zero "true" predicates when evaluating a CFS with no default
  - e.g. consider it an input error, abort the simulation; if evaluating
    deterioration, fall back to a "default value" on that attribute's definition
- "default values" in numeric attribute definitions
  - generally, only useful in initial total snapshot preparation when historical
    data is incomplete; but, could be used in other implementation-specific
    situations
- how to handle multiple project effects acting on a single attribute
  - e.g. consider it an input error, abort the simulation.

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

### Implementation details of AssetFox

- minimum project cost limit
- performance curve adjustment factors
- calculated field timing
- "pre-application" of the null treatment (just after deterioration, before
  project selection)

### Glossary of Terms & Structured Data

Italicized text is used to indicate parts of a data structure. When such text is
followed by a colon, the text after the colon clarifies the general type of data
in the named part. If the part is optional, a parenthesized question mark is
shown. If the part comprises other items, a parenthesized minimum count is
shown.

- **Simulation Time Sequence**
  - This is the abstract sequence of time steps over which the simulation
    operates.
- **Asset**
  - This is any entity that (a) may deteriorate over time and (b) may be managed
    or maintained by applying treatments to the entity.
  - Within the simulation, an asset is nothing more than a unique identifier.
- **Attribute Definition**
  - This describes a property associated with some or all assets in the
    simulation.
- **Asset Snapshot**
  - This is a set of values, one for each attribute definition, fully describing
    a single asset at a single moment in the simulation time sequence.
  - _Asset Identifier_
  - _Attribute Values_, each:
    - _Attribute Identifier_
    - _Value_
- **Total Snapshot**
  - This is a set of asset snapshots, one for each asset in the scenario, for a
    single step in the simulation time sequence.
- **Asset Function (Numeric)**
  - This is a function that applies to an asset snapshot and evaluates to a real
    number.
- **Asset Predicate**
  - This is a logical predicate that applies to an asset snapshot and evaluates
    to a classical truth value (i.e. either "true" or "false").
- **Conditional Function System**
  - This (abbreviated as CFS) is a "default" asset function alongside a set of
    asset predicate-function pairs.
  - The default function is optional if and only if there is at least one
    predicate-function pair.
- **Attribute Definition (Numeric)**
  - _Identifier_
  - _Minimum Value_ : number (?)
  - _Maximum Value_ : number (?)
  - _Deterioration Model_ : numeric CFS (?)
    - _Direction of Deterioration_ : negative or positive
  - SAF implementations may define other kinds of attribute, e.g. text or date,
    and they may also define "virtual attributes", e.g. based on asset
    functions. These are not essential to the SAF, but may be useful within a
    specific implementation.
- **Attribute Value Effect**
  - _Target Attribute_
  - _Effect Model_ : CFS
- **Future Treatment Requirement**
  - _Required Treatment_
  - _Completion Time Offset_ : positive integer
- **Null Treatment**
  - _Attribute Value Effects_ : (0+)
- **Treatment**
  - _Identifier_
  - _Attribute Value Effects_ : (1+)
  - _Future Treatment Requirements_ : (0+)
- **Selectable Treatment**
  - _Treatment_
  - _Cost Model_ : CFS
  - _Selection Predicate_ : (?)
  - _Allowed Budgets_ : (1+)
- **Prohibited-Concurrency Treatment Class**
  - _Applicability Predicate_ : (?)
  - _Mutually Exclusive Treatments_ : (2+)
- **Scenario**
  - This is a container of all input required for a single execution of a
    simulation algorithm.
  - _Attribute Definitions_ : (1+)
  - _Initial Total Snapshot_
  - _Null Treatment_
  - _Selectable Treatments_ : (1+)
  - _Prohibited-Concurrency Treatment Classes_ : (0+)
  - _Initial Committed Projects_ : (0+)
- **Project**
  - _Simulation Time Offset_ : non-negative integer
  - _Asset Identifier_
  - _Treatment_
  - _Funding Structure_
- **Selected Project**
  - _Simulation Time Offset_ : non-negative integer
  - _Asset Identifier_
  - _Treatment_
  - _Funding Structure_
- **Committed Project**
  - _Simulation Time Offset_ : non-negative integer
  - _Asset Identifier_
  - _Treatment_
  - _Funding Structure_
- **Project Option**
  - _Projects_
  - _Cost Per Time Step_
    - reflecting cash flow arrangement
  - _Estimated Selection Value_
    - filled in based on treatment outlook in RoadCare/AssetFox.
- **Project Activity**

### Output Data

_Project Plan_

### Data Operations

_Simulation_

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
