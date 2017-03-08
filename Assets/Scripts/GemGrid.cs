using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

public enum GridGemType
{
    Gem,
    Vegetable
}

public class GemGrid : MonoBehaviour 
{
    //private CooldownTimer timeAfterMatch;
    private CooldownTimer hintTimer;
    public float HintTimeout = 5.0f;   

    public class MatchOverrideRules
    {
        public GemColor? ByColor = null;
        public HashSet<Gem> IgnoreList = new HashSet<Gem>();

        public static MatchOverrideRules None { get { return new MatchOverrideRules(); } }        
    }

    public int GridDimensionsX = 8;
    public int GridDimensionsY = 8;

    public int Dims = 64;

    public Gem[,] gemGrid = null;

    public Camera Camera;

    private Gem[] vegetableResources = null;
    private List<Gem> currentGemSelection = new List<Gem>();

    public Gem Selected = null;

    private List<Gem> activeGems = new List<Gem>();    
    private bool gridMatchingIsActive = false;

    void Awake()
    {
        this.gemGrid = new Gem[GridDimensionsX, GridDimensionsY];

        this.vegetableResources = Resources.LoadAll<Gem>("Prefabs/Vegetables");
    }

	// Use this for initialization
	void Start() 
    {
        this.hintTimer = new CooldownTimer(HintTimeout, false);
        //this.timeAfterMatch = new CooldownTimer(GameManager.Instance.GetTotalGlowActivationWindow(), true);
    }

    public void PopulateGrid(int? gemLimit = null)
    {
        if (this.activeGems.Count > 0) 
        {
            GameUtils.LogToDebug(string.Format("Removing {0} gems", this.activeGems.Count));
            foreach (Gem gem in this.activeGems)
            {
                if (gem != null)
                {                    
                    this.StartCoroutine(gem.Vanish());
                }
            }
            
            this.activeGems.Clear();
        }

        this.currentGemSelection.Clear();

        this.currentGemSelection.AddRange(this.vegetableResources);
        
        if (gemLimit != null)
        {
            int diff = this.currentGemSelection.Count - gemLimit.Value;            

            for (int i = 0; i < diff; ++i)
            {
                // Remove gems until we reach limit
                Gem gem = this.currentGemSelection.GetRandom();
                this.currentGemSelection.Remove(gem);
            }

            if (PlayerManager.Instance.Bonuses.PurplePower)
            {
                // If no purples, swap out first gem with eggplant
                if (!this.currentGemSelection.Any(a => a.GemType == GemType.Eggplant))
                {
                    Gem eggplant = this.vegetableResources.ToList().FirstOrDefault(a => a.GemType == GemType.Eggplant);
                    if (eggplant != null)
                    {
                        this.currentGemSelection.Remove(this.currentGemSelection[0]);
                        this.currentGemSelection.Add(eggplant);
                    }
                }
            }
        }

        for (int x = 0; x < GridDimensionsX; ++x)
        {
            for (int y = 0; y < GridDimensionsY; ++y)
            {
                Gem gem = null;
                while (true)
                {
                    // Keep trying to get a random gem until it creates no match
                    gem = this.GetRandomGemInstance();
                    if (this.CreatesMatches(gem, x, y))
                    {
                        Destroy(gem.gameObject);
                    }
                    else
                    {
                        break;
                    }
                }

                gem.transform.parent = this.transform;
                this.MoveGemTo(gem, x, y);
                this.activeGems.Add(gem);
            }
        }
    }

    public void TransformGem(Gem gemToTransform, Gem newGem)
    {
        if (gemToTransform != null && newGem != null &&
            gemToTransform.gameObject != null && newGem.gameObject != null)
        {
            this.activeGems.Remove(gemToTransform);
            Vector3 pos = gemToTransform.transform.position;
            int x = gemToTransform.GridX;
            int y = gemToTransform.GridY;
            newGem.transform.SetParent(this.transform);
            newGem.Grid = this;
            this.activeGems.Add(newGem);
            Destroy(gemToTransform.gameObject);
            this.MoveGemTo(newGem, x, y, false, false);
        }
    }

    public void IrrigateAll()
    {
        foreach (Gem gem in this.activeGems)
        {
            gem.Irrigate();
        }
    }

    public List<Gem> ActiveGems { get { return this.activeGems; } }

    private static int globalId = 0;

    private Gem GetRandomGemInstance()
    {
        Gem gem = null;
        LevelGoal level = GameManager.Instance.GetLevelGoal();

        float badLuckMultiplier = PlayerManager.Instance.Bonuses.LuckyCharmEnabled ? 0.5f : 1.0f;
        float goodLuckMultiplier = PlayerManager.Instance.Bonuses.LuckyCharmEnabled ? 2.0f : 1.0f;

        float freezeEvent = level.FreezeGemProbability * badLuckMultiplier;
        float weedsEvent = freezeEvent + level.WeedsProbability * badLuckMultiplier;
        float redOreEvent = weedsEvent + level.RedOreProbability * goodLuckMultiplier;
        float poisonOreEvent = redOreEvent + level.PoisonOreProbability * badLuckMultiplier;
        float redLeafProb = poisonOreEvent + level.RedLeafProbability * badLuckMultiplier;
        float timeStoneEvent = redLeafProb + level.TimeStoneProbability * goodLuckMultiplier;

        float rand = UnityEngine.Random.Range(0.0f, 1.0f);
        if (rand < freezeEvent)
        {
            gem = GameManager.Instance.WeedSettings.FreezeGemTemplate;
        }
        else if (rand < weedsEvent)
        {
            gem = GameManager.Instance.WeedSettings.WeedsTemplate;
        }
        else if (rand < redOreEvent)
        {
            gem = GameManager.Instance.WeedSettings.RedOreTemplate;
        }
        else if (rand < poisonOreEvent)
        {
            gem = GameManager.Instance.WeedSettings.PoisonRockTemplate;
        }
        else if (rand < redLeafProb)
        {
            gem = GameManager.Instance.WeedSettings.RedWeedsTemplate;
        }
        else if (rand < timeStoneEvent)
        {
            gem = GameManager.Instance.WeedSettings.TimeStoneTemplate;
        }
        else
        {
            gem = this.currentGemSelection.GetRandom();
        }

        Gem instance = Instantiate(gem);
        instance.Grid = this;
        instance.GemId = globalId++;
        return instance;        
    }

    public void MoveGemTo(Gem gem, int x, int y, bool slide = false, bool outsideGrid = false)
    {
        if (!outsideGrid)
        {
            if (x >= 0 && y >= 0)
            {
                this.gemGrid[x, y] = gem;
            }
        }

        gem.SetGridLoc(x, y);

        Vector3 destination = new Vector3(x + 0.5f, -y - 0.5f, 0);
        if (slide)
        {
            gem.SlideGemTo(destination);
        }
        else
        {
            gem.transform.localPosition = destination;
        }
    }

    public float GetTotalSlideSpeed()
    {
        return GameManager.Instance.SlideSpeed * PlayerManager.Instance.Bonuses.FastDropMultiplierBonus;
    }
	
	// Update is called once per frame
	void Update () {

        if (GameManager.Instance.IsPaused)
        {
            return;
        }

        //this.timeAfterMatch.Tick(Time.deltaTime);

        if (this.CanMakeMove() && !GameManager.Instance.IsPaused)
        {
            if (this.hintTimer.Tick(Time.deltaTime).IsExpired)
            {
                this.hintTimer.Reset();
                Gem hintGem = this.LookForPossibleMatch();
                if (hintGem != null && hintGem.gameObject != null)
                {
                    GameManager.Instance.CreateHintCallout(hintGem);
                }
            }
        }
    }

    public void SetGlowActicationWindow(float window)
    {
        //this.timeAfterMatch.SetBaseline(window);
    }

    public bool CanSwapWith(Gem gem1, Gem gem2)
    {
        if (gem1 == null || gem2 == null)
        {
            return false;
        }        

        if (gem1 == gem2)
        {
            return false;
        }

        if (!this.AreAdjacent(gem1, gem2))
        {
            return false;
        }

        if (!this.IsLegalSwap(gem1, gem2))
        {
            return false;
        }

        return true;
    }

    public void DestroyOnClick(Gem gem)
    {
        this.StartCoroutine(this.ProcessMatches(new List<Gem>() { gem }));
    }

    public bool TrySwapWith(Gem gem1, Gem gem2)
    {
        if (this.CanSwapWith(gem1, gem2))
        {
            this.gridMatchingIsActive = true;

            List<List<Gem>> matches = this.GetMatchesOnSwap(gem1, gem2);
            this.SwapGems(gem1, gem2);
            gem1.SetSelected(false);
            gem2.SetSelected(false);
            this.Selected = null;

            // If this option was toggled, untoggle it
            GameManager.Instance.NextSwapFree = false;

            if (matches != null && matches.Count > 0)
            {
                this.StartCoroutine(this.ProcessMatches(matches));
                //this.timeAfterMatch.Reset();                
                //GameManager.Instance.AfterIrrigation(this.timeAfterMatch);
                this.hintTimer.Reset();
            }
            else
            {
                this.gridMatchingIsActive = false;
            }

            return true;
        }

        return false;
    }

    private IEnumerator ProcessMatches(List<Gem> matches, bool getRewards = true, MatchOverrideRules rules = null)
    {
        return ProcessMatches(new List<List<Gem>> { matches }, getRewards, rules);
    }

    private IEnumerator ProcessMatches(List<List<Gem>> matchGroups, bool getRewards = true, MatchOverrideRules rules = null)
    {        
        gridMatchingIsActive = true;
        
        if (rules == null)
        {
            rules = MatchOverrideRules.None;
        }

        GameManager.Instance.ClearHintCallouts();

        List<List<Gem>> tempMatchGroups = new List<List<Gem>>();
        List<Gem> allMatches = new List<Gem>();
        foreach (List<Gem> group in matchGroups)
        {
            // Create temp lists
            allMatches.AddRange(group);
            tempMatchGroups.Add(group);
        }

        do
        {
            HashSet<Gem> additionalMatches = new HashSet<Gem>();
            rules.IgnoreList = additionalMatches;

            while (allMatches.Any(gem => gem.InTransition))
            {
                yield return null;
            }

            // HACK: This check is for a really rare race condition that sadly i can't find.
            // If this happens, ignore this round and check again for matches. It's
            // almost a try/catch.
            if (!allMatches.Any(gem => gem == null))
            {
                if (getRewards)
                {
                    foreach (List<Gem> group in tempMatchGroups)
                    {
                        if (group.Count > 0)
                        {
                            if (group.Any(gem => gem == null))
                            {
                                // Try extra hard to avoid nulls!
                                continue;
                            }

                            GameManager.Instance.ProcessMatchRewards(new List<Gem>(group));
                        }
                    }
                }
                else
                {
                    // After first round of "matches", subsequent matches
                    //  resulting from drop should get rewards
                    getRewards = true;
                }

                foreach (Gem match in allMatches)
                {
                    this.RemoveGem(match);
                }

                SoundManager.Instance.PlaySound(SoundEffects.Pop);
            }

            this.DropRows();

            while (this.AreGemsInTransition())
            {
                yield return null;
            }

            allMatches = new List<Gem>();
            tempMatchGroups = new List<List<Gem>>();

            // Get all additional resulting match sets, add each to list
            foreach (List<Gem> matchSet in this.GetAllMatches(rules))
            {
                tempMatchGroups.Add(matchSet);
                additionalMatches.AddRange(matchSet);
            }

            if (additionalMatches.Count > 0)
            {
                // Add additional resulting maches to grand total
                allMatches.AddRange(additionalMatches);
            }
        } while (allMatches.Count > 0); 

        gridMatchingIsActive = false;
    }

    public bool HasMatches(MatchOverrideRules rules = null)
    {
        return this.GetAllMatches(rules).Count > 0;
    }

    /// <summary>
    /// Gets a list of match sets. Each match set is one 
    /// </summary>
    /// <param name="rules"></param>
    /// <returns></returns>
    private List<List<Gem>> GetAllMatches(MatchOverrideRules rules = null)
    {
        rules = rules ?? MatchOverrideRules.None;
        List<List<Gem>> matches = new List<List<Gem>>();
        for (int x = 0; x < GridDimensionsX; ++x)
        {
            for (int y = 0; y < GridDimensionsY; ++y)
            {
                Gem current = this.gemGrid[x, y];
                if (!rules.IgnoreList.Contains(current))
                {
                    List<Gem> matchGroup = this.GetMatches(current, x, y, rules);
                    if (matchGroup.Count > 0)
                    {              
                        GameUtils.LogCoordConcat("Adding matches", matchGroup);
                        matches.Add(matchGroup);
                        rules.IgnoreList.AddRange(matchGroup);
                    }
                }
            }
        }        

        return matches;
    }           

    private void DropRows()
    {
        // Find number of empty squares under each previously existing gem;
        // Start from the bottom up!
        for (int x = 0; x < GridDimensionsX; ++x)
        {
            for (int y = GridDimensionsY - 1; y >= 0; --y)
            {
                int numEmpty = 0;
                if (this.gemGrid[x, y] == null)
                {
                    continue;
                }

                for (int scanY = y; scanY < GridDimensionsY; ++scanY)
                {
                    if (this.gemGrid[x, scanY] == null)
                    {
                        numEmpty++;
                    }
                }

                if (numEmpty > 0)
                {
                    GameUtils.LogToDebug(string.Format("Dropping {0} down to [{1},{2}]", GameUtils.GetGemLogStats(this.gemGrid[x, y]), x, y + numEmpty));
                    this.MoveGemTo(this.gemGrid[x, y], x, y + numEmpty, true);
                    this.gemGrid[x, y] = null;
                }
            }
        }

        // Spawn new dudes        
        for (int x = 0; x < GridDimensionsX; ++x)
        {
            int numEmpty = 0;
            for (int y = 0; y < GridDimensionsY; ++y)
            {
                if (this.gemGrid[x, y] == null)
                {
                    numEmpty++;
                }
            }

            // plop them at the top and then slide them down
            int newY = 0;
            for (int spawn = 0; spawn < numEmpty; ++spawn)
            {
                newY--;
                Gem newGem = this.GetRandomGemInstance();
                newGem.transform.parent = this.transform;
                this.MoveGemTo(newGem, x, newY, false, true); // plop at top
                this.MoveGemTo(newGem, x, newY + numEmpty, true); // specify destination
                this.activeGems.Add(newGem);
                GameUtils.LogToDebug(string.Format("Creating gem {0}", GameUtils.GetGemLogStats(newGem)));
            }
        }               
    }

    private List<List<Gem>> GetMatchesOnSwap(Gem gem1, Gem gem2,
        MatchOverrideRules gem1Rules = null, MatchOverrideRules gem2Rules = null)
    {
        List<Gem> matches1 = this.GetMatches(gem1, gem2.GridX, gem2.GridY, gem1Rules);
        List<Gem> matches2 = this.GetMatches(gem2, gem1.GridX, gem1.GridY, gem2Rules);
        return new List<List<Gem>> { matches1, matches2 };
    }

    public void SwapGems(Gem gem1, Gem gem2)
    {
        // Temp for swap
        int gem1GridX = gem1.GridX;
        int gem1GridY = gem1.GridY;
        this.MoveGemTo(gem1, gem2.GridX, gem2.GridY, true);
        this.MoveGemTo(gem2, gem1GridX, gem1GridY, true);        
    }

    public bool CreatesMatches(Gem gem, int targetX, int targetY)
    {
        return this.GetMatches(gem, targetX, targetY).Count > 0;
    }

    /// <summary>
    /// Checks if the swap is legal based on 3 things: 
    /// 1) Free swap is enabled
    /// 2) A regular match is created
    /// 3) Color swap is enabled and the swap is legal from that
    /// </summary>
    /// <param name="gem1"></param>
    /// <param name="gem2"></param>
    /// <returns></returns>
    public bool IsLegalSwap(Gem gem1, Gem gem2)
    {
        MatchOverrideRules rules1 = null;
        MatchOverrideRules rules2 = null;
        if (GameManager.Instance.IsColorSwapEnabled)
        {
            rules1 = new MatchOverrideRules() { ByColor = gem1.GemColor };
            rules2 = new MatchOverrideRules() { ByColor = gem2.GemColor };
        }

        return GameManager.Instance.NextSwapFree ||             
            this.GetMatchesOnSwap(gem1, gem2, rules1, rules2).Any(a => a.Count > 0);
    }

    public List<Gem> GetMatches(Gem gem, int targetX, int targetY, MatchOverrideRules additionalRules = null) 
    {
        if (!gem.CanMatchThree)
        {
            return new List<Gem>();
        }

        if (additionalRules == null) 
        {
            additionalRules = MatchOverrideRules.None;
        }

        Func<Gem, bool> IsInIgnore = (Gem currentGem) =>
        {
            if (additionalRules.IgnoreList == null || 
                additionalRules.IgnoreList.Count == 0)
            {
                return false;
            }

            return additionalRules.IgnoreList.Contains(currentGem);
        };

        Func<Gem, bool> IsSuccessfullMatch = (Gem currentGem) =>
        {
            if (IsInIgnore(currentGem))
            {
                return false;
            }
            
            if (gem.IsMatch(currentGem))
            {
                return true;
            }

            if (additionalRules.ByColor != null &&
                gem.GemColor == additionalRules.ByColor.Value &&
                gem.IsColorMatch(currentGem))
            {
                return true;
            }

            return false;
        };

        List<Gem> verticalMatches = new List<Gem>();
        List<Gem> horizontalMatches = new List<Gem>();
        List<Gem> matches = new List<Gem>();

        Gem current = null;
        for (int x = targetX + 1; x < GridDimensionsX; ++x)
        {
            current = gemGrid[x, targetY];
            if (IsSuccessfullMatch(current))
            {
                horizontalMatches.Add(current);
            }
            else
            {
                break;
            }
        }

        for (int x = targetX - 1; x >= 0; --x)
        {
            current = gemGrid[x, targetY];
            if (IsSuccessfullMatch(current))
            {                
                horizontalMatches.Add(current);
            }
            else
            {
                break;
            }
        }

        for (int y = targetY + 1; y < GridDimensionsY; ++y)
        {
            current = gemGrid[targetX, y];
            if (IsSuccessfullMatch(current))
            {                
                verticalMatches.Add(current);
            }
            else
            {
                break;
            }
        }

        for (int y = targetY - 1; y >= 0; --y)
        {
            current = gemGrid[targetX, y];
            if (IsSuccessfullMatch(current))
            {                
                verticalMatches.Add(current);
            }
            else
            {
                break;
            }
        }

        if (verticalMatches.Count >= 2)
        {
            matches.AddRange(verticalMatches);
        }

        if (horizontalMatches.Count >= 2)
        {
            matches.AddRange(horizontalMatches);
        }

        if (matches.Count > 0)
        {
            matches.Add(gem);
        }

        return matches;
    }

    public Gem LookForPossibleMatch()
    {
        List<Gem> gemsToCheck = new List<Gem>(this.activeGems);
        while (gemsToCheck.Count > 0)
        {
            Gem gem = gemsToCheck.GetRandom();
            gemsToCheck.Remove(gem);
            if (gem.gameObject != null)
            {
                List<Gem> neighbors = this.GetNeighbors(gem); 
                foreach (Gem neighbor in neighbors)
                {
                    if (this.CreatesMatches(gem, neighbor.GridX, neighbor.GridY))
                    {                        
                        return gem;
                    }
                }                           
            }
        }

        return null;
    }

    public List<Gem> GetNeighbors(Gem gem)
    {
        List<Gem> gems = new List<Gem>();
        if (gem != null)
        {            
            if (gem.GridX > 0)
            {
                gems.Add(this.gemGrid[gem.GridX - 1, gem.GridY]);
            }
            if (gem.GridY > 0)
            {
                gems.Add(this.gemGrid[gem.GridX, gem.GridY - 1]);
            }
            if (gem.GridX < this.GridDimensionsX - 1)
            {
                gems.Add(this.gemGrid[gem.GridX + 1, gem.GridY]);
            }
            if (gem.GridY < this.GridDimensionsY - 1)
            {
                gems.Add(this.gemGrid[gem.GridX, gem.GridY + 1]);
            }
        }

        return gems;
    }

    private bool AreAdjacent(Gem gem1, Gem gem2)
    {
        int diffX = Mathf.Abs(gem1.GridX - gem2.GridX);
        int diffY = Mathf.Abs(gem1.GridY - gem2.GridY);

        if ((diffX == 1 && diffY == 0) ||
            (diffY == 1 && diffX == 0))
        {
            return true;
        }

        return false;
    }

    public IEnumerator RemoveAllGems(GemType gemType)
    {
        while (!CanMakeMove())
        {
            yield return null;
        }

        gridMatchingIsActive = true;

        List<Gem> toRemove = this.activeGems.Where(a => a.GemType == gemType).ToList();

        this.StartCoroutine(this.ProcessMatches(toRemove, false));
    }

    public IEnumerator RemoveAllGems(GemColor gemColor)
    {
        while (!CanMakeMove())
        {
            yield return null;
        }

        gridMatchingIsActive = true;
        List<Gem> toRemove = null;
        if (gemColor == GemColor.Half)
        {
            toRemove = new List<Gem>(this.activeGems);
            for (int i = 0; i < this.activeGems.Count / 2; ++i)
            {
                // Randomly remove half of the gems
                Gem gem = toRemove.GetRandom();
                toRemove.Remove(gem);
            }
        }
        else if (gemColor == GemColor.Undesirables)
        {
            toRemove = new List<Gem>();
            foreach (Gem gem in this.activeGems)
            {
                // Randomly remove half of the gems
                if (gem.IsAWeed || gem.GemType == GemType.PoisonRock || gem.GemType == GemType.FreezeGem)
                {
                    toRemove.Add(gem);
                }
            }
        }
        else
        {
            toRemove = this.activeGems.Where(a => a.GemColor == gemColor).ToList();
        }

        if (toRemove != null)
        {
            this.StartCoroutine(this.ProcessMatches(toRemove, false));
        }
    }

    public IEnumerator MatchByColor(GemColor gemColor)
    {
        while (!CanMakeMove())
        {
            yield return null;
        }

        gridMatchingIsActive = true;

        List<List<Gem>> matchSets = this.GetAllMatches(new MatchOverrideRules() { ByColor = gemColor });

        if (matchSets.Count > 0)
        {
            foreach (List<Gem> matchSet in matchSets)
            {
                GameUtils.LogCoordConcat("Processing matches", matchSet);
                this.StartCoroutine(this.ProcessMatches(matchSet, true));
            }
        }
        else
        {
            gridMatchingIsActive = false;
        }
    }    

    private void RemoveGem(Gem match)
    {
        if (match != null)
        {
            GameUtils.LogToDebug(string.Format("Removing gem: {0}", GameUtils.GetGemLogStats(match)));
            this.gemGrid[match.GridX, match.GridY] = null;
            this.activeGems.Remove(match);
            this.StartCoroutine(match.Vanish());            
        }
    }

    public bool SoonAfterMatch()
    {
        //return !this.timeAfterMatch.IsExpired;
        return false;
    }

    public bool CanMakeMove()
    {
        return !gridMatchingIsActive && !AreGemsInTransition();
    }

    public bool AreGemsInTransition()
    {
        return this.activeGems.Any(a => a.InTransition);
    }
}


