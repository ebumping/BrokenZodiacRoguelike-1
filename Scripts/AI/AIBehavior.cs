using Godot;
using System;
using CodexOfTheBrokenZodiac.AI;

namespace CodexOfTheBrokenZodiac.AI
{
    public partial class AIBehavior : Node
    {
        // Behavior configuration
        [Export] public float AggressionFactor { get; set; } = 1.0f;
        [Export] public float DefensiveFactor { get; set; } = 1.0f;
        [Export] public float WanderSpeedFactor { get; set; } = 1.0f;
        [Export] public float ChaseSpeedFactor { get; set; } = 1.0f;
        [Export] public float SpecialAttackProbability { get; set; } = 0.3f;
        [Export] public bool PreferRangedCombat { get; set; } = false;
        [Export] public bool UseCover { get; set; } = false;
        [Export] public bool UseAmbushTactics { get; set; } = false;
        [Export] public bool PatrolsArea { get; set; } = true;
        
        // Patrol path properties
        [Export] public NodePath PatrolPathNodePath { get; set; }
        private Path2D _patrolPath;
        private int _currentPatrolPointIndex = 0;
        
        // Group coordination
        [Export] public bool CoordinatesWithGroup { get; set; } = false;
        [Export] public float CoordinationRadius { get; set; } = 10.0f;
        
        // Decision making timers
        private float _decisionTimer = 0.0f;
        private const float DECISION_INTERVAL = 0.5f;
        
        // State tracking
        private EnemyController.AIState _lastRecommendedState = EnemyController.AIState.Idle;
        private bool _hasCalledForHelp = false;
        
        public override void _Ready()
        {
            // Initialize patrol path if specified
            if (PatrolPathNodePath != null)
            {
                _patrolPath = GetNode<Path2D>(PatrolPathNodePath);
            }
        }
        
        public void Process(EnemyController.AIState currentState, float delta, EnemyController controller)
        {
            // Update decision timer
            _decisionTimer -= delta;
            
            // Make decisions at regular intervals to avoid constant state changes
            if (_decisionTimer <= 0)
            {
                _decisionTimer = DECISION_INTERVAL;
                MakeDecision(currentState, controller);
            }
            
            // Process behavior specific to current state
            switch (currentState)
            {
                case EnemyController.AIState.Patrol:
                    ProcessPatrolBehavior(delta, controller);
                    break;
                    
                case EnemyController.AIState.Chase:
                    ProcessChaseBehavior(delta, controller);
                    break;
                    
                case EnemyController.AIState.TakeCover:
                    ProcessCoverBehavior(delta, controller);
                    break;
                    
                case EnemyController.AIState.Flee:
                    ProcessFleeBehavior(delta, controller);
                    break;
            }
        }
        
        private void MakeDecision(EnemyController.AIState currentState, EnemyController controller)
        {
            // Get enemy properties through reflection (simplification)
            Node2D currentTarget = (Node2D)controller.Get("_currentTarget");
            bool canAttack = (bool)controller.Get("_canAttack");
            bool canSpecialAttack = (bool)controller.Get("_canSpecialAttack");
            float currentHealth = (float)controller.Get("_currentHealth");
            float maxHealth = (float)controller.Get("_maxHealth");
            float healthPercentage = currentHealth / maxHealth;
            
            // Select next state based on current situation
            EnemyController.AIState recommendedState = currentState;
            
            // If no target, patrol or idle
            if (currentTarget == null)
            {
                recommendedState = PatrolsArea ? EnemyController.AIState.Patrol : EnemyController.AIState.Idle;
            }
            else
            {
                // Calculate distance to target
                float distanceToTarget = controller.GlobalPosition.DistanceTo(currentTarget.GlobalPosition);
                float attackRange = 1.5f; // Default attack range
                
                // Try to get actual attack range from controller
                var enemyData = controller.Get("_enemyData");
                if (enemyData != null)
                {
                    attackRange = (float)enemyData.Get("AttackRange");
                }
                
                // Check if we can attack
                bool inAttackRange = distanceToTarget <= attackRange;
                
                // Health-based decisions
                if (healthPercentage < 0.3f && DefensiveFactor > 0.5f)
                {
                    // Low health, be defensive
                    if (UseCover)
                    {
                        recommendedState = EnemyController.AIState.TakeCover;
                    }
                    else
                    {
                        recommendedState = EnemyController.AIState.Flee;
                    }
                }
                // Attack decisions
                else if (inAttackRange && canAttack)
                {
                    // Use special attack sometimes if available
                    if (canSpecialAttack && GD.Randf() < SpecialAttackProbability)
                    {
                        recommendedState = EnemyController.AIState.SpecialAttack;
                    }
                    else
                    {
                        recommendedState = EnemyController.AIState.Attack;
                    }
                }
                // Chase decision
                else
                {
                    recommendedState = EnemyController.AIState.Chase;
                    
                    // Call for help if aggressive and haven't already
                    if (AggressionFactor > 0.7f && CoordinatesWithGroup && !_hasCalledForHelp)
                    {
                        CallForHelp(controller);
                    }
                }
                
                // Ranged enemies might prefer to maintain distance
                if (PreferRangedCombat && distanceToTarget < attackRange * 0.7f)
                {
                    recommendedState = EnemyController.AIState.Flee;
                }
            }
            
            // Only change state if it's different from current
            if (recommendedState != currentState && recommendedState != _lastRecommendedState)
            {
                _lastRecommendedState = recommendedState;
                controller.Call("SetState", (int)recommendedState);
            }
        }
        
        private void ProcessPatrolBehavior(float delta, EnemyController controller)
        {
            // If we have a patrol path, follow it
            if (_patrolPath != null)
            {
                // Get current patrol point
                var curvePoints = _patrolPath.Curve.GetBakedPoints();
                if (curvePoints.Length > 0)
                {
                    Vector2 targetPoint = _patrolPath.GlobalPosition + curvePoints[_currentPatrolPointIndex];
                    float distanceToPoint = controller.GlobalPosition.DistanceTo(targetPoint);
                    
                    // If reached current point, move to next
                    if (distanceToPoint < 20.0f)
                    {
                        _currentPatrolPointIndex = (_currentPatrolPointIndex + 1) % curvePoints.Length;
                    }
                    
                    // Set the target point
                    controller.Set("_wanderTarget", targetPoint);
                }
            }
        }
        
        private void ProcessChaseBehavior(float delta, EnemyController controller)
        {
            // Advanced chase behavior here
            // For now, just using the basic chase in enemy controller
        }
        
        private void ProcessCoverBehavior(float delta, EnemyController controller)
        {
            // Find cover points in the environment
            // This would require environment analysis
            // For now, just a placeholder
        }
        
        private void ProcessFleeBehavior(float delta, EnemyController controller)
        {
            // Could implement more complex flee behavior
            // Like finding hiding spots or escape routes
        }
        
        private void CallForHelp(EnemyController controller)
        {
            _hasCalledForHelp = true;
            
            // Find nearby allies and alert them
            var enemies = controller.GetTree().GetNodesInGroup("Enemies");
            foreach (Node enemy in enemies)
            {
                if (enemy is EnemyController allyController && allyController != controller)
                {
                    float distance = controller.GlobalPosition.DistanceTo(allyController.GlobalPosition);
                    
                    if (distance <= CoordinationRadius)
                    {
                        // Alert this ally to our target
                        var currentTarget = controller.Get("_currentTarget");
                        if (currentTarget != null)
                        {
                            // Set the ally's target to our target
                            allyController.Set("_currentTarget", currentTarget);
                            allyController.Call("SetState", (int)EnemyController.AIState.Chase);
                        }
                    }
                }
            }
        }
        
        // Utility method to find cover points
        public Vector2 FindNearestCoverPoint(Vector2 currentPosition, Vector2 threatPosition)
        {
            // In a full implementation, this would analyze the environment
            // For now, just return a point opposite from the threat
            Vector2 awayDirection = (currentPosition - threatPosition).Normalized();
            return currentPosition + awayDirection * 100.0f;
        }
    }
}
