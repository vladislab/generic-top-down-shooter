using System.Collections;
/*This class is a static class which takes in a Generic Array and a Seed number
Then performs Fisher-Yates Shuffling Algorithm
Returns the shufflied Generic array */
public static class Utilities
{
   
    public static T[] ShuffleArray<T>(T[] array, int seed){
        System.Random prng= new System.Random(seed);
        //Fisher-Yates shuffle method
        for(int i=0 ; i<array.Length-1;i++){
            int randomIndex = prng.Next(i,array.Length);
            T tempItem = array[randomIndex];
            array[randomIndex] = array[i];
            array[i] = tempItem;
        }
        return array;
    }

    
}
