# iAM Simulation Algorithm Framework (SAF)

> The purpose of this document is not to describe any specific software
> application or any specific body of software code. The purpose is to describe
> a general framework allowing various software implementations of one overall
> approach to a simulation algorithm that assists asset management project
> planning.

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

- We want something that's both high-level and detailed. We want to know the
  purpose of things like "calculated field timing", but we don't necessarily
  care about describing how exactly that thing gets handled at the
  code/book-keeping level.
- Also, note we're distinguishing the overall framework algorithm from the
  yearly decision-making (project-picking) algorithm. Each has its own
  required/suppplemental inputs and outputs.

## sketch of content

### inputs

- To the simulation, there is no network or "explorer". It's just a set of
  assets and a set of attributes.
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
  attribute(s) in its specification, there's no need to have "special"
  attributes.
- alternative methods of defining functions may exist, e.g. "piecewise
  expressions" as used by RoadCare and AssetFox.

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
  - This is any entity whose physical attributes (a) may deteriorate over time
    and (b) may be managed or maintained by applying treatments to the entity.
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
  - This (abbreviated CFS) is a default asset function alongside a set of asset
    predicate-function pairs.
  - The default function is optional if and only if there is at least one
    predicate-function pair.
  - A CFS behaves like an individual asset function, in that it maps an asset
    snapshot to a real number based on the results of the CFS predicates.
- **Attribute Definition (Numeric)**
  - _Identifier_
  - _Minimum Value_ : number (?)
  - _Maximum Value_ : number (?)
  - _Deterioration Model_ : numeric CFS (?)
    - _Direction of Deterioration_ : negative or positive
  - SAF implementations may define other kinds of attribute, e.g. text or date,
    and they may also define "virtual attributes", e.g. based on asset
    functions. These are not essential to the SAF, but may be useful to provide
    additional features within a specific implementation.
- **Attribute Value Effect**
  - _Target Attribute_
  - _Effect Model_ : CFS
- **Prioritized Future Treatment Funding Rule**
  - _Treatment Funding Rule_
  - _Relative Time Offset_ : positive integer
- **Future Treatment Exclusion Rule**
  - _Excluded Treatments_ : Treatment (1+)
  - _Exclusion Period Length_ : positive integer
- **Null Treatment**
  - _Attribute Value Effects_ : (0+)
- **Treatment**
  - _Identifier_
  - _Attribute Value Effects_ : (1+)
- **Treatment Funding Rule**
  - _Applicability Predicate_ : (?)
  - _Treatment_
  - _Cost Model_ : CFS
  - _Allowed Budgets_ : (1+)
  - _Future Treatment Exclusion Rules_ : (0+)
  - _Prioritized Future Treatment Funding Rules_ : (0+)
- **Treatment Concurrency Exclusion Rule**
  - _Applicability Predicate_ : (?)
  - _Mutually Exclusive Treatments_ : (2+)
- **Scenario**
  - This is a container of all input required for a single execution of a
    simulation algorithm.
    - Some input data is transitively included, e.g. treatments.
  - _Attribute Definitions_ : (1+)
  - _Initial Total Snapshot_
  - _Committed Projects_ : (0+)
  - _Null Treatment_
  - _Treatment Funding Rules_ : (1+)
  - _Treatment Concurrency Exclusion Rules_ : (0+)
  - _BUDGETS_
  - _CONDITION GOALS_
  - _CASH FLOW RULES_
  - **Configuration**
    - project option generation logic (TFR -> PO)
      - project option value estimation logic
    - project option selection logic
- **Project Option**
  - _Source_ : Treatment Funding Rule
  - _Asset Identifier_
  - _Cost Sequence_ : (1+)
    - multiple via cash-flow
  - _Non-deteriorating Attributes_ : (0+)
    - applicable if two or more costs in sequence
    - but where does it come from? the treatment? the effects?
    - maybe it's an implementation detail. but cash-flow is woefully incomplete
      without this configurability.
  - _Objective Value_
    - estimated total "value" for the usual selection mode of maximizing value
      (derived from "benefit" or "remaining life") while respecting budget
      constraints
    - estimated total cost for the less usual selection mode of minimizing cost
      while achieving goal constraints and respecting budget constraints
- **Project Activity**
  - _Source_ : Project Option
  - _Simulation Time Step_ : positive integer
  - _Funding Structure_
    - a set of budget-amount pairs, where the amount is the money spent from the
      budget to fund this project.

### Output Data

At the framework level (ignoring additional output requirements imposed by SAF
implementations), the only output is a project activity matrix.

_Initial Total Snapshot_ + _Project Activity Matrix_ = _Final Total Snapshot_

### Data Operations

_Simulation_

- start with last step's "final values"
- DETERIORATION
  - carries the attributes into the current phase of decision-making
  - an asset or its attributes may not deteriorate under certain conditions,
    e.g. active cash-flow.
- determine project options, including null project options based on the null
  treatment
  - There needs to be a single function that maps from TFR to PO. The
    implementation of this function does not need to be defined at the framework
    level, but it should be defined separately at least for the existing
    implementation in AssetFox.
  - check treatment feasibility criteria, shadows, etc
  - could also consider 2 different cash flow periods as 2 different project
    options
- estimate the "objective value" of each project option
  - PLUG-IN: needs an input/output spec
- determine selected projects (i.e. decide on funding, e.g. budgets, cash-flow?)
  - PLUG-IN: needs an input/output spec
- PROJECT EFFECTS
- done; next time step
