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

This is the overall _conceptual structure_ of the data and logic in the iAM SAF.
Individual SAF implementations (SAFIs) may use additional data or less data, and
they may adjust/add/remove some logic.

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

### Glossary of Terms & Structured Data

Italicized text is used to indicate parts of a data structure. When such text is
followed by a colon, the text after the colon clarifies the general type of data
in the named part. If the part is optional, a parenthesized question mark is
shown. If the part comprises other items, a parenthesized minimum count is
shown.

Also, some of the structures shown here have explicit "identifier" fields, which
is really only to make it clear that instances of those structures are referred
to in other structures. However, in a given implementation, all structures could
have identifiers, and it's probably beneficial to make it so.

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
  - This is a set of values fully describing a single asset at a single moment
    in the simulation time sequence.
  - This does not necessarily have a value for each attribute, as some
    attributes may be inapplicable to some assets.
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
- **Asset Function System (AFS)**
  - This is a default asset function alongside a set of asset predicate-function
    pairs.
  - The default function is optional if and only if there is at least one
    predicate-function pair.
  - An AFS **is** an asset function. It maps an asset snapshot to a real number
    based on the results of the predicates.
  - Abstractly, an AFS can **contain** other AFS. Whether and how to do this is
    up to the SAFI.
  - A specific AFS can be made to do anything with its internal functions. Here
    are some examples of AFS types:
    - **Switching AFS**
      - This AFS produces the result of exactly one function in the AFS.
      - If there are multiple satisfied predicates, that's considered an input
        error.
      - If there are no results (no default and no satisfied predicates), that's
        considered an input error.
    - **Summing AFS**
      - This AFS produces the result of summing the default function result (if
        present) with all results from functions whose predicates are satisfied.
      - If there are no results (no default and no satisfied predicates), that's
        considered an input error.
    - **Minimizing AFS**
      - This AFS produces the minimum result among all results from the default
        function and any functions whose predicates are satisfied.
      - If there are no results (no default and no satisfied predicates), that's
        considered an input error.
    - **Maximizing AFS**
      - This AFS produces the maximum result among all results from the default
        function and any functions whose predicates are satisfied.
      - If there are no results (no default and no satisfied predicates), that's
        considered an input error.
- **Attribute Definition (Numeric)**
  - _Identifier_
  - _Minimum Value_ : number (?)
  - _Maximum Value_ : number (?)
  - _Deterioration Model_ : asset function (?)
    - _Direction of Deterioration_ : negative or positive
      - This is probably unnecessary in general? What framework-level parts
        actually require it?
  - SAFIs may define other kinds of attribute, e.g. text or date, and they may
    also define "virtual attributes", e.g. based on asset functions. These are
    not essential to the SAF, but may be useful to provide additional features
    within a specific SAFI.
- **Attribute Value Effect**
  - _Target Attribute_
  - _Effect Model_ : asset function
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
  - _Non-deteriorating Attributes_ : (0+)
    - applicable if part of a multi-step project
- **Treatment Funding Rule**
  - _Applicability Predicate_ : (?)
  - _Treatment_
  - _Cost Model_ : asset function
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
  - _Budgets_
  - _Cash Flow Rules_
  - _CONDITION GOALS_
  - **Configuration**
    - _Allow multiple budgets to fund a single project_
    - _Allow budget remainders to carry forward_
- **Budget**
  - _Applicability Predicate_ : (?)
  - _Investment Sequence_ : (1+)
- **Cash Flow Rule**
  - _Applicability Predicate_ : (?)
  - _Distribution Rules_, each:
    - _Cost Ceiling_
    - _Percentage Sequence_ : (1+)
- **Project Option**
  - _Source Rule_ : Treatment Funding Rule
  - _Asset Identifier_
  - _Cost Sequence_ : (1+)
    - multiple via cash-flow
  - _Objective Value_
    - possible interpretations:
      - estimated total "value" for the usual selection mode of maximizing value
        (derived from "benefit" or "remaining life") while respecting budget
        constraints
      - estimated total cost for the less usual selection mode of minimizing
        cost while achieving goal constraints and respecting budget constraints
        - further bits are needed to evaluate goal constraints
- **Project**
  - _Simulation Time Step_ : positive integer
  - _Asset Identifier_
  - _Treatment_
  - _Funding Structure_
    - basically a matrix of numeric entries. columns are relative time steps,
      rows are budgets, and entries are amounts spent from a budget in a time
      step.

outstanding modeling problems & solutions:

- prioritization handling needs to be kept simple. all that prioritization means
  is that you get a "first round" of decisions that uses only the current crop
  of prioritized TFRs on the respective assets.
  - "deferrals" are something to consider, e.g. when a prioritized TFR can't
    apply "in time" due to applicability predicate failure or due to
    non-selection of its project option (i.e. budget limitations or lack of
    necessity re goals), it's simply pushed back to the next time step.
- treatment concurrency needs to be the default mode. specifically, we cannot
  think of a cash-flow period as a hands-off period (which it is, currently in
  AssetFox). things can still be worked on. the solution is to lean back on
  concurrency exclusion rules, with the added sub-rule that a treatment cannot
  be re-applied while the treatment is being performed, and the treatment is
  "present" for exclusion-checking during the cash-flow.
- how to handle multiple project effects acting on a single attribute?
  - e.g. consider it an input error, abort the simulation.
  - Yeah, even if the two effects are coming from different projects, it kinda
    implies that the two treatments should have been mutually exclusive.

### Responsibilities of the SAFI

- (possibly) how to generate a project option from a TFR
  - really, I don't think we should "officially" open this up. if the objective
    value estimation needs extra stuff from this step, that's fine, the SAFI can
    allow the extra stuff to be tracked/calculated.
  - keeping this aspect closed and well-defined will make the documentation that
    much clearer and the framework that much more usable.
- how to estimate "objective value" of project options
  - basically the "outlook algorithm"
- how to select from a set of project options
  - basically the "consideration loop"

### Output Data

At the framework level (ignoring additional output requirements imposed by SAF
implementations), the only output is a set of projects.

_Initial Total Snapshot_ + _Projects (Committed & Selected)_ = _Final Total Snapshot_

A design goal is that the user should be able to easily (with little or no
effort) feed some or all of the output project activity _back into_ the
simulation as "committed project" input. In other words, when the user is
crafting committed project input, they should be crafting nearly the same
structure as what appears in the output. Note that this flexibility needs to
also include cash-flow. Users need to be able to clearly and intuitively specify
cash-flow committed projects.
- This has been achieved, I think.

### Data Operations

_Simulation_

1. DETERIORATION
2. PRIORITIZED PROJECT SELECTION
   - Using per-asset prioritized TFRs only, determine project options, estimate
     objective values, and determine selections.
3. PROJECT SELECTION
   - Using all TFRs over all assets, determine project options, estimate
     objective values, and determine selections.
4. PROJECT EFFECTS
   - Apply the effects of completed projects.
