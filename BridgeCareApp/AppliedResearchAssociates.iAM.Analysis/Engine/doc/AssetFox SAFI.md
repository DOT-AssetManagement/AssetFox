# AssetFox SAFI <!-- omit in toc -->

AssetFox is an asset management web application that contains an SAFI (i.e. an
implementation of the iAM SAF).

- [1. Overview of SAF Implementation](#1-overview-of-saf-implementation)
  - [1.1. EOV Methods](#11-eov-methods)
    - [1.1.1. EOV Calculation for Cumulative Benefit](#111-eov-calculation-for-cumulative-benefit)
    - [1.1.2. EOV Calculation for Minimum Remaining Life](#112-eov-calculation-for-minimum-remaining-life)
  - [1.2. Project Option Selection](#12-project-option-selection)
- [2. Unique SAFI Terms \& Corresponding SAF Concepts](#2-unique-safi-terms--corresponding-saf-concepts)
- [3. SAFI-Specific Terms \& Definitions](#3-safi-specific-terms--definitions)
- [4. Expressing Predicates \& Functions](#4-expressing-predicates--functions)
- [5. Pending Clarifications](#5-pending-clarifications)
- [6. Suggestions](#6-suggestions)

## 1. Overview of SAF Implementation

### 1.1. EOV Methods

This SAFI provides two primary EOV methods:

- _Cumulative Benefit_
- _Minimum Remaining Life_

Both of these EOV methods perform their calculations during an _outlook_
sub-phase, which is basically a partial simulation localized on the asset of the
project option whose EOV is being calculated. Outlook goes forward from the
moment of EOV calculation through a fixed number of time steps (typically 100,
but this is potentially configurable). Each time step uses only phase 1 and an
extended phase 4. Outlook begins in the current phase of the current time step
by assuming the selection only of the project option whose EOV is being
calculated. In each subsequent time step of outlook, phase 4 is extended to
include immediate application of all treatments associated with that time step's
currently prioritized project option creation rules. If the EOV method is
configured to produce the EOV as an EOV-cost ratio, the total cost of each such
treatment is accumulated into a running total seeded with the total cost of the
project option whose EOV is being calculated.

#### 1.1.1. EOV Calculation for Cumulative Benefit

To calculate cumulative benefit, additional scenario configuration is required.
A _benefit attribute_ must be designated, along with a _benefit limit_ and an
optionally designated _benefit weighting attribute_.

During outlook, the _benefit value_ time series is collected. Each benefit value
is calculated immediately after phase 4 (project completion) and is equal to the
distance from the current value of the benefit attribute to the benefit limit in
the direction of deterioration for the benefit attribute. If this distance is
negative (i.e. the current value of the benefit attribute is deteriorated beyond
the benefit limit), the benefit value is zero. If a benefit weighting attribute
has been designated, the benefit value is multiplied by the current value of the
benefit weighting attribute.

At the end of outlook, the cumulative benefit EOV is equal to the numeric
integral of the piecewise linear spline defined by the benefit value time
series.

#### 1.1.2. EOV Calculation for Minimum Remaining Life

To calculate minimum remaining life, additional scenario configuration is
required. One or more _remaining life rules_ must be defined. Each rule
designates a deteriorating attribute, a limit, and a predicate determining to
which assets the rule applies. At least one rule must apply when a project
option's EOV is being calculated.

During outlook, for each applicable rule, the attribute value time series is
collected. Each attribute value is taken immediately after phase 4 (project
completion) and is compared to the limit. If the attribute value has
deteriorated beyond the limit, the _remaining life value_ for that rule is equal
to the number of preceding **full** time steps in the outlook, which may be
zero. (The time step in which EOV is calculated is not a full time step in the
outlook.) If there is a preceding attribute value in the time series, a
fractional term is added to the remaining life. This fractional term is equal to
$(a_L - a) / (a' - a)$, where $a$ is the preceding attribute value, $a'$ is the
current attribute value, and $a_L$ is the limit.

At the end of outlook, if there are any applicable rules whose remaining life
value has not yet been calculated, the remaining life value for each such rule
is equal to the number of time steps configured for outlook. The minimum
remaining life EOV is equal to the minimum remaining life value among all
applicable rules.

### 1.2. Project Option Selection

Project option selection in this SAFI is performed using a greedy algorithm
looping over all project options in descending order of EOV. This loop is called
the "consideration loop" and each project option undergoes "consideration".

The consideration loop requires additional configuration in the scenario. A
_budget order_ must be defined, and zero or more _budget priority levels_ may be
defined. (See below for a list of terms and definitions including these.)

For each budget priority level, (a) the respective portion of each budget is
made available for constructing funding matrices and (b) a full consideration
loop is executed. If no budget priority levels are defined, a single
consideration loop is executed using the full amount of each budget.

On each consideration, the project option is accepted as a project if and only
if both of the following conditions are met:

- another project has not already been accepted for the same asset in the
  current time step;
- a valid funding matrix can be constructed to cover the cost sequence of the
  project option.

Normally, a funding matrix is valid if and only if it does not cause any
budget's remaining amount to go negative (further subject to any current
adjustment due to a budget priority level). However, this SAFI allows the
configuration of an "unlimited spending" mode by which this constraint can be
disabled, as well as a "no spending" mode to fully disable all project option
selection.

If the project option under consideration is accepted as a project, all budgets'
remaining amounts are decreased according to the project's funding matrix, and
the asset is marked as having a project in the current time step. Regardless,
consideration proceeds to the next project option.

The scenario may be configured to allow the consideration loop(s) to terminate
early if the constraints imposed by the scenario's goal conditions have been
satisfied by the effects of the set of projects accepted so far in the current
time step.

## 2. Unique SAFI Terms & Corresponding SAF Concepts

| SAFI Term                           | SAF Concept                                     |
| ----------------------------------- | ----------------------------------------------- |
| Active Treatment                    | treatment                                       |
| Asset Group                         | -                                               |
| Attribute Default Value             | -                                               |
| Attribute Value History             | -                                               |
| Benefit                             | an interpretation of EOV                        |
| Budget Order                        | -                                               |
| Budget Priority Level               | -                                               |
| Calculated Field                    | -                                               |
| Cash Flow Distribution Rule         | cost distribution grade                         |
| Cash Flow Rule                      | cost distribution rule                          |
| Criterion                           | predicate                                       |
| Equation                            | function                                        |
| Inflation Rate                      | -                                               |
| Minimum Project Cost Limit          | -                                               |
| Network                             | assets                                          |
| Number Attribute                    | attribute definition                            |
| Optimization Strategy               | EOV method                                      |
| Passive Treatment                   | -                                               |
| Performance Curve                   | deterioration model function                    |
| Performance Curve Adjustment Factor | -                                               |
| Performance Curve Shift Mode        | -                                               |
| Remaining Life                      | an interpretation of EOV                        |
| Roll-Forward                        | -                                               |
| Selectable Treatment                | project option creation rule                    |
| Spending Strategy                   | project option selection method                 |
| Text Attribute                      | -                                               |
| Treatment Bundle                    | implicit concurrency of projects                |
| Treatment Consequence               | attribute value function                        |
| Treatment Consideration Loop        | project option selection                        |
| Treatment Option                    | project option                                  |
| Treatment Outlook                   | the calculation of EOV                          |
| Treatment Scheduling                | future prioritized project option creation rule |
| Treatment Shadow                    | future treatment exclusion rule                 |
| Treatment Supersede Rule            | treatment concurrency exclusion rule            |

## 3. SAFI-Specific Terms & Definitions

- **Asset Group**
  - For a given asset group ID, this is the subset of all assets whose "group
    attribute" value is equal to the group ID.
  - The "group attribute" is specified as part of the scenario, is applicable to
    all assets, and is unchanging over the course of the simulation.
- **Attribute Default Value**
  - This is an extension on an attribute definition.
  - This value is used to fill certain gaps in _attribute value history_ during
    _roll-forward_.
- **Attribute Value History**
  - This is an extension on a scenario, replacing the initial total snapshot.
  - Attribute value history is used to accomplish _roll-forward_, which is a
    preprocessing step that calculates the initial total snapshot.
  - Conceptually, attribute value history is a set of 4-tuples, each with the
    following shape:
    - `(time step, asset, attribute, value)`
  - Attribute value history can contain any asset's attribute values for the
    first simulation time step and for time steps before the first simulation
    time step.
  - Each value in the history is interpreted as being from a snapshot taken just
    after phase 1 (deterioration) of the corresponding time step.
    - This allows the effects of deterioration in the first simulation time step
      to be ignored.
- **Budget Order**
  - This is the order in which budgets are examined when the SAFI is attempting
    to construct the funding matrix for selection of a project option under
    consideration.
- **Budget Priority Level**
  - This is a description of what portion of each budget is available at a
    specified "level" of priority during project option selection.
- **Calculated Field**
  - This is a "virtual" attribute definition, one whose values are always
    calculated from other attribute values in a given asset snapshot.
- **Inflation Rate**
  - This is used to adjust treatment costs upward as the simulation progresses
    through its time step sequence.
- **Minimum Project Cost Limit**
  - Evaluation of a project option creation rule is aborted if the rule's
    treatment's total cost is below this limit.
- **Passive Treatment**
  - Sometimes referred to as "the no-treatment".
  - This is intended to represent an asset's attribute value changes in a single
    time step when no project is selected for that asset.
  - This "treatment" is usually responsible for incrementing an "age" attribute,
    but a deterioration model can be used to accomplish the same thing just as
    easily.
- **Performance Curve Adjustment Factor**
  - On a single asset, the adjustment factor associated with a performance curve
    modifies the rate of deterioration _if that curve's function is defined by
    an age-dependent spline_.
  - A factor above one decreases the rate of deterioration and a factor below
    one increases the rate of deterioration. For example, if an asset has aged
    by 1.0 (which is the typical aging from one time step to the next), an
    adjustment factor of 2.0 will effectively change the age difference to 0.5.
  - An adjustment factor is associated with a performance curve by the
    application of treatments to an asset. Each asset maintains its own set of
    associations.
- **Performance Curve Shift Mode**
  - This is a mode associated with each performance curve, specified as part of
    the scenario and unchanging over the course of the simulation.
  - Shift mode is only applicable to a performance curve _if that curve's
    function is defined by an age-dependent spline_.
  - Conceptually, shift mode is intended to allow an attribute to deterioriate
    based on its current value rather than based on the asset's age, since it is
    possible for an asset to under-perform or over-perform relative to age-based
    expectations.
    - Note that deterioration based on current value is already possible (and
      more naturally formulated) by defining the curve's function with a
      mathematical expression (which has the entire asset snapshot in scope)
      rather than a piecewise expression (which has only the "age" attribute in
      scope).
  - When shift mode is enabled for a performance curve, the deterioration
    calculation is slightly modified. Normally, the deteriorating asset's "age"
    value is mapped directly to an attribute value on the spline, and then that
    value replaces the current value of that attribute. In this mode, the
    current attribute value on the asset is mapped to an "apparent age" value
    via the spline's inverse function. (It is an input requirement that all
    performance curve splines be invertible.) The "previous age" is then
    calculated (using the curve adjustment factor, if present) and used to
    determine a ratio of apparent age to previous age. The current age is
    multiplied by this ratio, giving a "shifted age" which is finally mapped to
    the new attribute value on the spline.
- **Roll-Forward**
  - This is a preprocessing step that translates the SAFI's _attribute value
    history_ into the SAF's required _initial total snapshot_.
  - Roll-forward is basically a sequence of partial simulation time steps that
    happen before the first simulation time step. During each of these time
    steps, only phases 1 (deterioriation) and 4 (project completion) occur. In
    other words, there is no project option selection during roll-forward.
    - Committed projects are allowed during roll-forward.
  - The first roll-forward time step is the earliest time step in the attribute
    value history. Attribute values on each asset are pulled from that time step
    in history.
    - If there is no value for a certain attribute on a certain asset in the
      first time step, then the value "falls forward" to the earliest historical
      time step with a value for that attribute on that asset. If there is no
      such time step, the attribute's default value is used.
  - The first roll-forward time step skips phase 1 because historical attribute
    values are interpreted as being from snapshots just after phase 1 of each
    time step.
  - On each subsequent roll-forward time step, just after phase 1, each
    attribute value is replaced if and only if the history contains a value for
    that attribute on the corresponding asset in the current time step.
- **Text Attribute**
  - This is another kind of attribute definition provided by this SAFI.
  - A text attribute has none of the properties of SAF numeric attributes.
  - A text attribute can have a default value.

## 4. Expressing Predicates & Functions

This SAFI primarily uses a mathematical expression language called
"Calculate-Evaluate" to allow flexible definition of its predicates (i.e.
"criteria) and functions (i.e. "equations").

A small "change expression" language is available for simpler definition of
attribute value functions on treatments.

A basic "piecewise expression" language is also available for defining
functions, usually deterioration functions. Such an expression contains a series
of 2D points defining a piecewise linear spline. Within an asset snapshot, the
corresponding function has exactly one dependency attribute, "age", an attribute
definition for which the scenario is required to provide.

## 5. Pending Clarifications

- **Calculated Field Timing**
  - The purpose of this aspect of input configuration is unclear. Non-default
    "timing" of a calculated field's evaluation doesn't seem to have a
    conceptual basis.
- **Passive Treatment**
  - Given the flexibility of deterioration model function definition, the
    justification for this aspect of the input is unclear.
  - This is perhaps a legacy from older versions of the RoadCare SAFI having
    only piecewise expressions for defining deterioration functions as
    age-dependent splines. If such versions existed, then the natural
    advancement of age had no representation other than being a consequence of
    this "treatment".
  - **"Pre-application" after phase 1**
    - Allegedly for compatibility with the RoadCare SAFI. Regardless, the domain
      concept and justification for this behavior is unclear.

## 6. Suggestions

- The outlook sub-phase could be simplified by discarding the extension on phase
  4 (project completion). EOV is only an estimate, and the phase 4 extension's
  direct bypass of the project option selection process in order to account for
  the possible effects of prioritized project options may be too optimistic and
  may violate treatment exclusion conditions. To keep outlook as-is while
  complying with exclusion conditions, outlook would probably have to
  significantly increase in complexity, approaching the requirements of full
  simulation.
