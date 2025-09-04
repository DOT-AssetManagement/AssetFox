# iAM Simulation Algorithm Framework (SAF)

## plan of writing

- We want something that's both high-level and detailed. We want to know why
  things like "calculated field timing" exist (answer to that one: no idea), but
  we don't necessarily care about describing how exactly that thing gets handled
  at the code/book-keeping level.
- Also, note we're distinguishing the overall framework algorithm from the
  yearly decision-making (project-picking) algorithm. Each has its own
  required/suppplemental inputs and outputs.

## sketch of content

- inputs
  - To the simulation, there is no network or "explorer". It's just a set of
    assets and a set of attributes.
  - assets
    - ID, name, description, whatever
    - list of attribute-value pairs
  - attribute
    - either number (double) or text (string)
      - number type should probably be decimal, but it doesn't matter for the
        sake of this doc.
    - also, "calculated fields"
    - min, max
      - no "default" within the simulation, as it's only used for attribute
        value initialization.
    - deterioration/performance curves
      - target attribute (number only; not calculated)
      - criterion for an asset to satisfy in order for this curve to apply to
        the target attribute
      - either interpolated function of special automatic "age" attribute,
      - or "equation" expression that's a function of some subset of the asset's
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
  - initial attribute values on each asset
    - This is not necessarily a part of simulation logic. Currently, the
      simulation handles it, but it should be decoupled to a preprocessing step.
  - algorithm-specific metadata, like budget priority levels, budget order, etc.
    - These things could be discarded if the algorithm changed. Other inputs
      cannot be discarded; they are essential to achieving the outputs.
  - time step definition (traditionally, "year", but the time step is handled
    very abstractly)
  - other settings:
    - yearly budget remainder allowed to roll forward to next year?
    - multiple budgets allowed to fund a single treatment?
    - assets allowed to deteriorate during cash-flow?
- flow per time step
  - start with last step's "final values"
  - deterioration applied where applicable
    - carries the attributes into the current phase of decision-making
    - generally, "events" need to be applied, e.g. committed/scheduled projects.
    - an asset may not deteriorate under certain conditions, e.g. active
      cash-flow.
  - estimate the "objective value" of each treatment for each asset.
    - allow for optional bundling of certain combinations of treatments, to be
      treated as a single treatment.
      - alternatively, just generally allow the selection of multiple treatments
        for an asset, with treatments allowed to specify either (a) the other
        treatments they can combine with or (b) the other treatments they can't
        combine with.
    - including the "no treatment" option.
  - pick (and decide on funding, e.g. budgets, cash-flow)
    - PLUG-IN (theoretically)
    - input/output spec here, too
  - apply
  - done; next time step
- outputs
  - a project event schedule that shows what treatment decisions were made when
    and what the effects of those decisions were (on the attribute values of
    each asset)
  - "reasons" why each decision was made
    - This would be completely obviated by using CP for the decision phase of
      each year.
    - This is an example of accidental output, specific to the "picker"
      algorithm.
