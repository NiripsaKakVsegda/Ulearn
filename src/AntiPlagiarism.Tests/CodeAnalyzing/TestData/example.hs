 calc :: String -> Float
 calc = head . foldl f [] . words
   where 
     f :: [Float] -> String -> [Float]
     f (x:y:zs) "+"    = (y + x):zs
     f (x:y:zs) "-"    = (y - x):zs
     f (x:y:zs) "*"    = (y * x):zs
     f (x:y:zs) "/"    = (y / x):zs
     f (x:y:zs) "FLIP" = y:x:zs
     f (x:zs)   "ABS"  = (abs x):zs
     f xs       y      = read y : xs