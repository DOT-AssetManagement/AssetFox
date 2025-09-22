# AssetFox's SAFI <!-- omit in toc -->

AssetFox is an asset management web application that contains an implementation
of the iAM SAF.

## Overview of Method Implementations

### EOV Calculation

### Project Option Selection

## Unique SAFI Terms & Corresponding SAF Concepts

| SAFI Term                           | SAF Concept                                     |
| ----------------------------------- | ----------------------------------------------- |
| Active Treatment                    | treatment                                       |
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
| Performance Curve Shift             | -                                               |
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

## SAFI-Specific Terms & Definitions

- **Attribute Default Value**
  - This is an extension on an attribute definition.
  - This value is used to fill certain gaps in _attribute value history_ during
    _roll-forward_.
- **Attribute Value History**
  - This is an extension on a scenario, replacing the initial total snapshot.
  - The attribute value history is used to accomplish _roll-forward_, which is a
    preprocessing step that calculates the initial total snapshot.
  - The attribute value history can contain asset attribute values for the first
    simulation time step and for time steps before the first simulation time
    step.
  - Each value in the history is interpreted as being from a snapshot taken just
    after phase 1 (deterioration) of the corresponding time step. In other
    words, it allows overriding the effects of deterioration in the first
    simulation time step.
- **Benefit**
- **Budget Order**
  - This is the order in which budgets are examined when the SAFI is attempting
    to construct the funding matrix for selection of a project option under
    consideration.
- **Budget Priority Level**
  - This is a description of what portions of all budgets are available at a
    specified level of priority during project option selection.
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
    modifies the rate of deterioration _if that performance curve is defined by
    an age-dependent spline_.
  - A factor above one decreases the rate of deterioration and a factor below
    one increases the rate of deterioration. For example, if an asset has aged
    by 1.0 time steps, an adjustment factor of 2.0 will effectively change the
    age difference to 0.5 time steps.
  - An adjustment factor is associated with a performance curve by the
    application of treatments to an asset. Each asset maintains its own set of
    associations.
- **Performance Curve Shift**
  - This is a mode associated with each performance curve, specified as part of
    the scenario and unchanging over the course of the simulation.
  - When enabled, 

## Expressing Criteria & Equations

This SAFI primarily uses an external expression language called
"Calculate-Evaluate" to allow flexible definition of its criteria and
equations.

A small "change expression" language is available for simpler definitions of
attribute value functions on treatments.

A basic "piecewise expression" language is also available for defining
functions, particularly deterioration functions. Such an expression contains a
series of 2D points defining a linear spline. Within an asset snapshot, the
corresponding function has exactly one dependency attribute, "age", an attribute
definition for which the scenario is required to provide.

## Pending Clarifications

- **Calculated Field Timing**
  - Why? Post-deterioration is useless and pre-deterioration is conceptually
    unmoored.
- **Passive Treatment**
  - Why? Deterioration suffices.
  - This is perhaps a legacy of older versions of the RoadCare SAFI having only
    age-dependent piecewise expressions for defining deterioration functions, in
    which case "age deterioration" had no other representation.

---

# sketch/draft content



## differences that are part of the EOV/selection logic

- benefit
  - attribute
  - limit
- remaining life
  - attribute
  - limit
- "outlook" & its configuration
  - number of years
- "consideration" & its configuration
  - budget order
  - budget priority levels

## differences that are insane and probably wrong/buggy/obsolete

- calculated field timing
- "pre-application" of passive treatment

### misc remarks

- the quasi-required "age" attribute is not technically required. It's only used
  to facilitate handling of piecewise expressions as performance curves. In
  other words, if that kind of "special" expression could include the dependency
  attribute(s) in its specification, there's no need to have "special"
  attributes.
- alternative methods of defining functions may exist, e.g. "piecewise
  expressions" as used by RoadCare and AssetFox.

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
- "AGE" attribute
- age-dependent "piecewise" expressions for interpolated functions in
  deterioration models
- "value-change" expressions in treatment consequences
- "outlook" algorithm and its configuration, e.g. length of time etc.
- "calculate-evaluate" expression language for user-friendly construction of
  asset functions and predicates
- benefit weighting, spatial weighting
- inflation rate percentage

### Implementation details of AssetFox

- In AssetFox, cost models are (via a global option) allowed to be summing AFSs
- performance curve adjustment factors
- calculated field timing
- "pre-application" of the null treatment (just after deterioration, before
  project selection)
  - frankly, this looks like an awkward attempt to patch up incomplete
    deterioration rules
- minimum project cost limit
